module Reports

open System
open Database
open MySql.Data.MySqlClient

let totalSalesReport () =
    use connection = Database.connect()
    let query = "SELECT SUM(TotalCost) AS TotalSales FROM `Order`"
    use command = new MySqlCommand(query, connection)
    use reader = command.ExecuteReader()
    if reader.Read() then
        let totalSales = reader.GetDecimal(0)
        printfn "Total Sales: %.2f" totalSales
    else
        printfn "No sales found."

let inventoryValueReport () =
    use connection = Database.connect()
    let query = "SELECT SUM(Price * Quantity) AS InventoryValue FROM Product"
    use command = new MySqlCommand(query, connection)
    use reader = command.ExecuteReader()
    if reader.Read() then
        let inventoryValue = reader.GetDecimal(0)
        printfn "Total Inventory Value: %.2f" inventoryValue
    else
        printfn "No products found."