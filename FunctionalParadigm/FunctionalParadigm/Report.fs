module Report

open System
open Database
open MySql.Data.MySqlClient

// Utility function to execute a query and return a single result
let private executeSingleReader query parameters mapFn =
    use connection = connect()
    use command = new MySqlCommand(query, connection)
    parameters |> List.iter (fun (key, value) -> command.Parameters.AddWithValue(key, value) |> ignore)
    use reader = command.ExecuteReader()
    if reader.Read() then Some(mapFn reader) else None

// Generate a total sales report
let totalSalesReport () =
    let query = "SELECT SUM(TotalCost) AS TotalSales FROM `Order`"
    try
        match executeSingleReader query [] (fun reader -> reader.GetDecimal(0)) with
        | Some totalSales -> printfn "Total Sales: %.2f" totalSales
        | None -> printfn "No sales found."
    with
    | ex -> printfn "Error generating total sales report: %s" ex.Message

// Generate an inventory value report
let inventoryValueReport () =
    let query = "SELECT SUM(Price * Quantity) AS InventoryValue FROM Product"
    try
        match executeSingleReader query [] (fun reader -> reader.GetDecimal(0)) with
        | Some inventoryValue -> printfn "Total Inventory Value: %.2f" inventoryValue
        | None -> printfn "No products found."
    with
    | ex -> printfn "Error generating inventory value report: %s" ex.Message


