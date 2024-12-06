module Program

open System
open ProductManagement
open OrderManagement
open Notification
open Report

let displayMenu () =
    printfn "Stock Management System"
    printfn "1. Add Product"
    printfn "2. Update Product"
    printfn "3. Delete Product"
    printfn "4. Display All Products"
    printfn "5. Make Order"
    printfn "6. Display Notifications"
    printfn "7. Reports"
    printfn "8. Exit"

let getProductDetails () =
    printfn "Enter product name:"
    let name = Console.ReadLine()

    printfn "Enter product price:"
    let price = Console.ReadLine() |> Decimal.Parse

    printfn "Enter product quantity:"
    let quantity = Console.ReadLine() |> Int32.Parse

    (name, price, quantity)

let handleChoice choice =
    match choice with
    | 1 ->
        let (name, price, quantity) = getProductDetails()
        addProduct name price quantity
    | 2 ->
        displayProducts()
        printfn "Enter Product ID to update:"
        let productID = Console.ReadLine() |> Int32.Parse
        updateProductDetails productID
    | 3 ->
        displayProducts()
        printfn "Enter Product ID to delete:"
        let productID = Console.ReadLine() |> Int32.Parse
        deleteProduct productID
    | 4 -> displayProducts()
    | 5 ->
        printfn "Enter customer name:"
        let customerName = Console.ReadLine()
        makeOrder customerName
    | 6 ->
        printfn "Notifications:"
        displayLowStockProducts()  // No more type mismatch here
    | 7 ->
        printfn "Please select a report:"
        printfn "1. Low-Stock Items Report"
        printfn "2. Total Sales Report"
        printfn "3. Inventory Value Report"
        printfn "Enter the number of your choice:"
        let reportChoice = Console.ReadLine() |> Int32.Parse
        match reportChoice with
        | 1 -> displayLowStockProducts()  // Calls the updated version
        | 2 -> totalSalesReport()
        | 3 -> inventoryValueReport()
        | _ -> printfn "Invalid report choice. Please try again."
    | 8 ->
        printfn "Exiting program..."
        Environment.Exit(0)
    | _ ->
        printfn "Invalid choice! Please try again."


// Recursive function to keep the menu running
let rec mainLoop () =
    displayMenu()
    printfn "Enter your choice:"
    let choice = Console.ReadLine() |> Int32.Parse
    handleChoice choice
    printfn ""
    mainLoop()  // Recurse to allow continuous input

[<EntryPoint>]
let main argv =
    mainLoop()
    0  // Return value of main




