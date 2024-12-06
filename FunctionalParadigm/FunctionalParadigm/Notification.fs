module Notification

open Database
open MySql.Data.MySqlClient

let threshold = 2

// Utility function to execute a query and return a list of results
let private executeReader query parameters mapFn =
    use connection = connect()
    use command = new MySqlCommand(query, connection)
    parameters |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
    use reader = command.ExecuteReader()
    [ while reader.Read() do yield mapFn reader ]

// Function to get low stock products
let getLowStockProducts () =
    let query = "SELECT Name, Quantity FROM Product WHERE Quantity <= @Threshold"
    let parameters = [("@Threshold", box threshold)]

    try
        let products = executeReader query parameters (fun reader ->
            (reader.GetString(0), reader.GetInt32(1))) // Map to tuple of product name and quantity
        Ok products
    with
    | ex -> Error (sprintf "Error fetching low stock products: %s" ex.Message)

// Function to display low stock products
let displayLowStockProducts () =
    match getLowStockProducts() with
    | Ok products when List.isEmpty products -> 
        printfn "No products are below the threshold."
    | Ok products -> 
        products
        |> List.iter (fun (name, quantity) -> printfn "Low stock: %s | Current quantity: %d" name quantity)
        printfn "Low stock products displayed."
    | Error msg -> 
        printfn "Error: %s" msg


