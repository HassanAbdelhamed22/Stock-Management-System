module ProductManagement

open System
open MySql.Data.MySqlClient
open Database

// A utility to execute a command and return the number of rows affected
let private executeNonQuery query parameters =
    use connection = connect()
    use command = new MySqlCommand(query, connection)
    parameters |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
    command.ExecuteNonQuery()

// A utility to execute a query and transform results into a list
let private executeReader query parameters mapFn =
    use connection = connect()
    use command = new MySqlCommand(query, connection)
    parameters |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
    use reader = command.ExecuteReader()
    [ while reader.Read() do yield mapFn reader ]


// Function to add a product
let addProduct name price quantity =
    let query = "INSERT INTO product (Name, Price, Quantity) VALUES (@name, @price, @quantity)"
    let rowsAffected = executeNonQuery query [("@name", box name); ("@price", box price); ("@quantity", box quantity)]
    if rowsAffected > 0 then
        printfn "Product added successfully."
    else
        printfn "Failed to add product."


// Function to update a product
let updateProduct productID (newName: string) (newPrice: decimal) (newQuantity: int) =
    let query = "UPDATE product SET Name = @name, Price = @price, Quantity = @quantity WHERE ProductID = @productID"
    let rowsAffected = executeNonQuery query [
        ("@name", box newName)
        ("@productID", box productID)
        ("@price", box newPrice)
        ("@quantity", box newQuantity)
    ]
    if rowsAffected > 0 then
        printfn "Product updated successfully."
    else
        printfn "Failed to update product."



// Function to get product details by ID
let getProductDetails productID =
    let query = "SELECT Name, Price, Quantity FROM product WHERE ProductID = @productID"
    let results = executeReader query [("@productID", productID)] (fun reader ->
        reader.GetString("Name"), reader.GetDecimal("Price"), reader.GetInt32("Quantity"))
    match results with
    | [name, price, quantity] -> Some(name, price, quantity)
    | _ -> None

// Interactive function to update product details
let updateProductDetails productID =
    match getProductDetails productID with
    | Some(currentName, currentPrice, currentQuantity) ->
        printfn "Current details: Name: %s | Price: %.2f | Quantity: %d" currentName currentPrice currentQuantity

        printfn "Enter new name (leave blank to keep):"
        let newName = Console.ReadLine()
        let finalName = if String.IsNullOrWhiteSpace(newName) then currentName else newName

        printfn "Enter new price (leave blank to keep):"
        let newPriceStr = Console.ReadLine()
        let finalPrice = if String.IsNullOrWhiteSpace(newPriceStr) then currentPrice else Decimal.Parse(newPriceStr)

        printfn "Enter new quantity (leave blank to keep):"
        let newQuantityStr = Console.ReadLine()
        let finalQuantity = if String.IsNullOrWhiteSpace(newQuantityStr) then currentQuantity else Int32.Parse(newQuantityStr)

        updateProduct productID finalName finalPrice finalQuantity
    | None -> printfn "Product not found."




// Function to delete a product
let deleteProduct productID =
    let query = "DELETE FROM product WHERE ProductID = @productID"
    let rowsAffected = executeNonQuery query [("@productID", productID)]
    if rowsAffected > 0 then
        printfn "Product removed successfully."
    else
        printfn "Failed to remove product."

// Function to display all products
let displayProducts () =
    let query = "SELECT ProductID, Name, Price, Quantity FROM product"
    let products = executeReader query [] (fun reader ->
        reader.GetInt32("ProductID"), reader.GetString("Name"), reader.GetDecimal("Price"), reader.GetInt32("Quantity"))
    match products with
    | [] -> printfn "No products found."
    | _ ->
        products |> List.iter (fun (id, name, price, quantity) ->
            printfn "ID: %d | Name: %s | Price: %.2f | Quantity: %d" id name price quantity)


