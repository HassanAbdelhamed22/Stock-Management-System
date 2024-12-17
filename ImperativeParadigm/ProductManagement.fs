module ProductManagement

open System
open Database
open MySql.Data.MySqlClient

let addProduct name price quantity =
    use connection = Database.connect()
    let query = "INSERT INTO product (Name, Price, Quantity) VALUES (@name, @price, @quantity)"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@name", name) |> ignore
    command.Parameters.AddWithValue("@price", price) |> ignore
    command.Parameters.AddWithValue("@quantity", quantity) |> ignore
    let rows = command.ExecuteNonQuery()
    if rows > 0 then
        printfn "Product added successfully."
    else
        printfn "Failed to add product."

let updateProduct productID newName newPrice newQuantity =
    use connection = Database.connect()
    let query = "UPDATE Product SET Name = @Name, Price = @Price, Quantity = @Quantity WHERE ProductID = @ProductID"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@ProductID", productID) |> ignore
    command.Parameters.AddWithValue("@name", newName) |> ignore
    command.Parameters.AddWithValue("@price", newPrice) |> ignore
    command.Parameters.AddWithValue("@quantity", newQuantity) |> ignore
    let rows = command.ExecuteNonQuery()
    if rows > 0 then
        printfn "Product updated successfully."
    else
        printfn "Failed to update product."

let getProductDetails productID =
    use connection = Database.connect()
    let query = "SELECT Name, Price, Quantity FROM Product WHERE ProductID = @ProductID"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@ProductID", productID) |> ignore
    use reader = command.ExecuteReader()
    
    if reader.Read() then
        let name = reader.GetString("Name")
        let price = reader.GetDecimal("Price")
        let quantity = reader.GetInt32("Quantity")
        Some(name, price, quantity)
    else
        None

let updateProductDetails productID =
    match getProductDetails productID with
    | Some (currentName, currentPrice, currentQuantity) ->
        printfn "Current name: %s" currentName
        printfn "Current price: %.2f" currentPrice
        printfn "Current quantity: %d" currentQuantity
        
        printfn "Enter new product name (press Enter to keep current):"
        let newName = Console.ReadLine()
        let finalName = if String.IsNullOrWhiteSpace(newName) then currentName else newName
        
        printfn "Enter new product price (press Enter to keep current):"
        let newPriceStr = Console.ReadLine()
        let finalPrice = 
            if String.IsNullOrWhiteSpace(newPriceStr) then currentPrice 
            else Decimal.Parse(newPriceStr)

        printfn "Enter new product quantity (press Enter to keep current):"
        let newQuantityStr = Console.ReadLine()
        let finalQuantity = 
            if String.IsNullOrWhiteSpace(newQuantityStr) then currentQuantity
            else Int32.Parse(newQuantityStr)
        
        updateProduct productID finalName finalPrice finalQuantity
    | None -> 
        printfn "Product not found."

let deleteProduct productID =
    use connection = Database.connect()
    let query = "DELETE FROM Product WHERE ProductID = @ProductID"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@ProductID", productID) |> ignore
    let rows = command.ExecuteNonQuery()
    if rows > 0 then
        printfn "Product removed successfully."
    else
        printfn "Failed to remove product."

let displayProducts () =
    use connection = Database.connect()
    let query = "SELECT * FROM Product"
    use command = new MySqlCommand(query, connection)
    use reader = command.ExecuteReader()
    if reader.HasRows then
        while reader.Read() do
            printfn "ID: %d | Name: %s | Price: %.2f | Quantity: %d"
                (reader.GetInt32("ProductID"))
                (reader.GetString("Name"))
                (reader.GetDecimal("Price"))
                (reader.GetInt32("Quantity"))
    else
        printfn "No products found."