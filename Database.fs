module Database

open System
open MySql.Data.MySqlClient

let connectionString = "Server=localhost;Database=stock_management;User Id=root;Password=;"

let connect() =
  let connection = new MySqlConnection(connectionString)
  connection.Open()
  connection