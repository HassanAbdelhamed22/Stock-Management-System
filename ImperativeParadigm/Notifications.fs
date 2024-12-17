module Notifications
open Database
open MySql.Data.MySqlClient

let threshold = 2

let displayLowStockProducts () =
    use connection = Database.connect()
    let query = "SELECT Name, Quantity FROM Product WHERE Quantity <= @Threshold;"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@Threshold", threshold) |> ignore
    use reader = command.ExecuteReader()
    if reader.HasRows then
        while reader.Read() do
            let productName = reader.GetString(0)
            let productQuantity = reader.GetInt32(1)
            printfn "Low stock: %s | Current quantity: %d" productName productQuantity
    else
        printfn "No products are below the threshold."