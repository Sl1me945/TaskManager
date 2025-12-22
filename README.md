# 📝 ToDoApp

A simple command-line ToDo application built with C# and .NET, featuring authentication, multiple task types, filtering, sorting, and search.  
Designed as a clean and extensible project following good architectural practices.


## 🚀 Features

### 🔐 Authentication
- Sign Up
- Sign In
- Sign Out

### 📌 Task Management
- ➕ Add Task  
- 📄 View All Tasks  
- ✏️ Update Task  
- ❌ Delete Task  
- ✔️ Mark Task as Completed  
- 🔍 Search Tasks by Keyword  
- 📅 Sort Tasks by Date  
- 🎚️ Filter Tasks by Completion  

### 🧩 Task Types
- Simple Task  
- Work Task (with project name)  
- Recurring Task (with interval)  


## 🏗️ Architecture
- **Domain** – core entities and enums  
- **Application** – task services and managers  
- **Infrastructure** – repositories and data access  
- **Presentation** – CLI interface


## 🛠️ Technologies Used
- C# / .NET 8  
- Clean Architecture  
- Microsoft.Extensions.Logging  


## 📦 Installation

### 1️⃣ Clone the repository
```bash
git clone https://github.com/Sl1me945/ToDoApp
cd ToDoApp
```

### 2️⃣ Restore dependencies
```bash
dotnet restore
```

### 3️⃣ Set your own JWT Secret Key (Development only)
```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "Your_Jwt_secret_key_At_least_32_bytes"
```
⚠️ JWT secret is required and is not stored in the repository by design.

### Make sure the application is running in Development environment:

Bash (Linux / macOS / Git Bash)
```bash
export DOTNET_ENVIRONMENT=Development
```

PowerShell (Windows)
```powershell
$env:DOTNET_ENVIRONMENT="Development"
```

cmd (Windows)
```cmd
set DOTNET_ENVIRONMENT=Development
```

### 4️⃣ Run the application
```bash
dotnet run
```
