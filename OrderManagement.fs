module OrderManagement

open System
open Database
open MySql.Data.MySqlClient

let checkProductAvailabilityByName productName quantityRequested =
    use connection = Database.connect()
    let query = "SELECT ProductID, Price, Quantity FROM Product WHERE Name = @Name"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@Name", productName) |> ignore
    use reader = command.ExecuteReader()
    if reader.Read() then
        let availableQuantity = reader.GetInt32("Quantity")
        availableQuantity >= quantityRequested
    else
        false

let createOrder customerName =
  use connection = Database.connect()
  let query = "INSERT INTO `Order` (CustomerName, OrderDate, TotalCost) VALUES (@CustomerName, @OrderDate, @TotalCost)"
  use command = new MySqlCommand(query, connection)
  command.Parameters.AddWithValue("@CustomerName", customerName) |> ignore
  command.Parameters.AddWithValue("@OrderDate", DateTime.Now) |> ignore
  command.Parameters.AddWithValue("@TotalCost", 0.0) |> ignore
  command.ExecuteNonQuery() |> ignore
  int command.LastInsertedId

let addOrderDetails orderID productName quantity =
  use connection = Database.connect()
  let query = "SELECT ProductID, Price, Quantity FROM Product WHERE Name = @ProductName"
  use command = new MySqlCommand(query, connection)
  command.Parameters.AddWithValue("@ProductName", productName) |> ignore
  use reader = command.ExecuteReader()
  if reader.Read() then
    let productID = reader.GetInt32("ProductID")
    let price = reader.GetDecimal("Price")
    reader.Close()

    let subTotal = decimal quantity * price
    let orderDetailsQuery = "INSERT INTO OrderDetail (OrderID, ProductID, QuantityOrdered, SubTotal) VALUES (@OrderID, @ProductID, @QuantityOrdered, @SubTotal)"
    use orderDetailsCommand = new MySqlCommand(orderDetailsQuery, connection)
    orderDetailsCommand.Parameters.AddWithValue("@OrderID", orderID) |> ignore
    orderDetailsCommand.Parameters.AddWithValue("@ProductID", productID) |> ignore
    orderDetailsCommand.Parameters.AddWithValue("@QuantityOrdered", quantity) |> ignore
    orderDetailsCommand.Parameters.AddWithValue("@SubTotal", subTotal) |> ignore
    orderDetailsCommand.ExecuteNonQuery() |> ignore
    subTotal
  else
    failwith "Product not found."

let deductProductQuantity productName quantity =
    use connection = Database.connect()
    let query = "UPDATE Product SET Quantity = Quantity - @Quantity WHERE Name = @Name"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@Name", productName) |> ignore
    command.Parameters.AddWithValue("@Quantity", quantity) |> ignore
    command.ExecuteNonQuery() |> ignore

let updateOrderTotalCost orderID totalCost =
    use connection = Database.connect()
    let query = "UPDATE `Order` SET TotalCost = @TotalCost WHERE OrderID = @OrderID"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@OrderID", orderID) |> ignore
    command.Parameters.AddWithValue("@TotalCost", totalCost) |> ignore
    command.ExecuteNonQuery() |> ignore

let makeOrder customerName =
    use connection = Database.connect()
    use transaction = connection.BeginTransaction()

    try
        let mutable continueAdding = true
        let orderID = createOrder customerName
        let mutable totalCost = 0.0M

        while continueAdding do
            printfn "Enter product name:"
            let productName = Console.ReadLine()

            printfn "Enter quantity:"
            let quantity = 
                try
                    Console.ReadLine() |> Int32.Parse
                with
                | :? FormatException ->
                    printfn "Invalid input for quantity. Please enter a valid number."
                    0
                | ex ->
                    printfn "An error occurred: %s" ex.Message
                    0

            if quantity > 0 then
                let isAvailable = checkProductAvailabilityByName productName quantity

                if not isAvailable then
                    printfn "Error: Product '%s' is out of stock or insufficient quantity." productName
                else
                    let subTotal = addOrderDetails orderID productName quantity
                    deductProductQuantity productName quantity
                    totalCost <- totalCost + subTotal
                    printfn "Added product '%s' (Quantity: %d) to the order. Subtotal: %.2f" productName quantity subTotal

            printfn "Do you want to add another product to the same order? (yes/no):"
            let choice = Console.ReadLine().ToLower()
            continueAdding <- (choice = "yes")

        updateOrderTotalCost orderID totalCost

        transaction.Commit()
        printfn "Order placed successfully for customer '%s' with OrderID: %d, Total Cost: %.2f" customerName orderID totalCost
    with
    | ex ->
        transaction.Rollback()
        printfn "Error: %s" ex.Message