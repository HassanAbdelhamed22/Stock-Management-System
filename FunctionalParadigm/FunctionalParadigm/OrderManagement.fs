module OrderManagement

open System
open Database
open MySql.Data.MySqlClient

// Utility to execute non-query commands and return the number of rows affected
let private executeNonQuery query parameters =
    use connection = connect()
    use command = new MySqlCommand(query, connection)
    parameters |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
    command.ExecuteNonQuery()

// Utility to execute a query and transform results into a list
let private executeReader query parameters mapFn =
    use connection = connect()
    use command = new MySqlCommand(query, connection)
    parameters |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
    use reader = command.ExecuteReader()
    [ while reader.Read() do yield mapFn reader ]

// Check product availability by name
let checkProductAvailabilityByName productName quantityRequested =
    let query = "SELECT Quantity FROM Product WHERE Name = @Name"
    let results = executeReader query [("@Name", box productName)] (fun reader -> reader.GetInt32("Quantity"))
    match results with
    | [availableQuantity] -> availableQuantity >= quantityRequested
    | _ -> false

// Create an order
let createOrder customerName =
    let query = "INSERT INTO `Order` (CustomerName, OrderDate, TotalCost) VALUES (@CustomerName, @OrderDate, @TotalCost)"
    executeNonQuery query [("@CustomerName", box customerName); ("@OrderDate", box DateTime.Now); ("@TotalCost", box 0.0M)]
    let lastInsertQuery = "SELECT LAST_INSERT_ID()"
    match executeReader lastInsertQuery [] (fun reader -> reader.GetInt32(0)) with
    | [orderID] -> Ok orderID
    | _ -> Error "Failed to retrieve the last inserted order ID."

// Add order details
let addOrderDetails orderID productName quantity =
    let query = "SELECT ProductID, Price FROM Product WHERE Name = @ProductName"
    match executeReader query [("@ProductName", box productName)] (fun reader ->
        reader.GetInt32("ProductID"), reader.GetDecimal("Price")) with
    | [(productID, price)] ->
        let subTotal = decimal quantity * price
        let orderDetailsQuery = 
            "INSERT INTO OrderDetail (OrderID, ProductID, QuantityOrdered, SubTotal) VALUES (@OrderID, @ProductID, @QuantityOrdered, @SubTotal)"
        let parameters = [
            ("@OrderID", box orderID)
            ("@ProductID", box productID)
            ("@QuantityOrdered", box quantity)
            ("@SubTotal", box subTotal)
        ]
        match executeNonQuery orderDetailsQuery parameters with
        | rowsAffected when rowsAffected > 0 -> Ok subTotal
        | _ -> Error "Failed to add order details."
    | _ -> Error (sprintf "Product '%s' not found." productName)

// Deduct product quantity
let deductProductQuantity productName quantity =
    let query = "UPDATE Product SET Quantity = Quantity - @Quantity WHERE Name = @Name"
    let parameters = [("@Name", box productName); ("@Quantity", box quantity)]
    match executeNonQuery query parameters with
    | rowsAffected when rowsAffected > 0 -> Ok ()
    | _ -> Error "Failed to deduct product quantity."

// Update order total cost
let updateOrderTotalCost orderID totalCost =
    let query = "UPDATE `Order` SET TotalCost = @TotalCost WHERE OrderID = @OrderID"
    let parameters = [("@OrderID", box orderID); ("@TotalCost", box totalCost)]
    match executeNonQuery query parameters with
    | rowsAffected when rowsAffected > 0 -> Ok ()
    | _ -> Error "Failed to update order total cost."

// Make an order
let makeOrder customerName =
    let rec addProducts orderID totalCost =
        printfn "Enter product name:"
        let productName = Console.ReadLine()

        printfn "Enter quantity:"
        let quantity =
            try Console.ReadLine() |> Int32.Parse
            with
            | :? FormatException -> 
                printfn "Invalid input for quantity. Please enter a valid number."
                0

        if quantity > 0 then
            if not (checkProductAvailabilityByName productName quantity) then
                printfn "Error: Product '%s' is out of stock or insufficient quantity." productName
                totalCost
            else
                match addOrderDetails orderID productName quantity with
                | Ok subTotal ->
                    match deductProductQuantity productName quantity with
                    | Ok _ ->
                        printfn "Added product '%s' (Quantity: %d) to the order. Subtotal: %.2f" productName quantity subTotal
                        totalCost + subTotal
                    | Error err -> 
                        printfn "Error: %s" err
                        totalCost
                | Error err -> 
                    printfn "Error: %s" err
                    totalCost
        else totalCost

    let rec askForAnotherProduct orderID totalCost =
        printfn "Do you want to add another product to the same order? (yes/no):"
        match Console.ReadLine().ToLower() with
        | "yes" -> 
            let newTotalCost = addProducts orderID totalCost
            askForAnotherProduct orderID newTotalCost
        | "no" -> totalCost
        | _ -> 
            printfn "Invalid input. Please enter 'yes' or 'no'."
            askForAnotherProduct orderID totalCost

    use connection = connect()
    use transaction = connection.BeginTransaction()
    try
        match createOrder customerName with
        | Ok orderID ->
            let totalCost = addProducts orderID 0.0M
            let finalCost = askForAnotherProduct orderID totalCost
            match updateOrderTotalCost orderID finalCost with
            | Ok _ -> 
                transaction.Commit()
                printfn "Order placed successfully for customer '%s' with OrderID: %d, Total Cost: %.2f" customerName orderID finalCost
            | Error err -> 
                printfn "Error: %s" err
                transaction.Rollback()
        | Error err -> 
            printfn "Error: %s" err
            transaction.Rollback()
    with
    | ex ->
        transaction.Rollback()
        printfn "Error: %s" ex.Message

