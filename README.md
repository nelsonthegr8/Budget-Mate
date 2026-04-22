# Budget-Mate

A .NET 9 MAUI Blazor Hybrid personal finance application for tracking income, expenses, accounts, and generating financial forecasts.

## Tech Stack

- **.NET 9 MAUI Blazor Hybrid** — cross-platform native app with Blazor web UI
- **MudBlazor** — Material Design component library for the UI
- **SQLite (sqlite-net-pcl)** — local on-device database
- **.NET MAUI Community Toolkit**

## Project Structure

```
Models/            Data models (SQLite tables)
Services/          LocalDbService – all database logic
Components/Pages/  Blazor pages (Home, Income, Expense, AccountValues, Forecast, Roads)
Components/Layout/ MainLayout & NavMenu
```

## Data Models

| Model | Table | Purpose |
|---|---|---|
| `IncomeExpense` | `IncomeExpense` | Stores individual income or expense line items (Name, Amount, Type = "Income"/"Expense"), linked to a RoadMap |
| `Accnts` | `Accounts` | Financial accounts (Savings, Checkings, Credit Card, Loan, Asset Value) with balances, linked to a RoadMap |
| `RoadMaps` | `RoadMaps` | Budgeting scenarios/plans. Tracks net savings amount, previous savings, net worth, and previous net worth. One is marked `isSelected` at a time |
| `Forecasts` | `Forecasts` | Monthly forecast entries with projected income, extra income, running total, and cash stack |
| `MainMenuCards` | `MainMenuCards` | Dashboard summary cards (Net Worth, Income, Expenses, Financial Forecast) with computed amounts |

## Core Logic (`LocalDbService`)

- **Main Menu Cards** — On first load, seeds four dashboard cards (Net Worth, Income, Expenses, Financial Forecast). Each time the home page loads, card amounts are recalculated:
  - **Net Worth** = sum of Savings + Checkings + Asset Value accounts minus Credit Card + Loan balances for the selected RoadMap
  - **Income** = sum of all Income-type entries
  - **Expenses** = sum of all Expense-type entries

- **Income & Expenses** — CRUD operations on the `IncomeExpense` table. When an item is added or removed, the parent RoadMap's `RoadMapSavingAmount` is recalculated as `SUM(Income) - SUM(Expense)` for that RoadMap.

- **Accounts** — CRUD on the `Accounts` table. Adding or removing an account automatically recalculates the parent RoadMap's `NetWorth`.

- **RoadMaps** — Budget scenarios that group income/expenses and accounts together. Deleting a RoadMap cascades and removes all its linked income/expense entries. A "Default" RoadMap is auto-created if none exist.

- **Forecasts** — Generate and save month-by-month financial projections. Users configure months/years to project, optionally select accounts as a starting cash stack, and the forecast table displays monthly savings, extra income, running total, and cumulative cash stack. Forecasts are paginated by year.

## Pages

| Route | Page | Description |
|---|---|---|
| `/` | **Home** | Dashboard with summary cards for Net Worth, Income, Expenses, and Forecast. Each card links to its detail page |
| `/income` | **Income** | Add/remove income entries. Shows a form and a data grid of current incomes |
| `/expense` | **Expense** | Add/remove expense entries tied to a selected RoadMap |
| `/accountvalues` | **Account Values** | Add/remove financial accounts (Savings, Checkings, Credit Card, Loan, Asset Value) tied to a RoadMap |
| `/forecast` | **Forecast** | Configure and generate month-by-month financial projections. Select a RoadMap, set time horizon, optionally pick starting accounts, then view/save the forecast |
| `/roads` | **Roads** | View and delete RoadMap budget scenarios |
