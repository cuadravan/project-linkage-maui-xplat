# Project Linkage App (.NET MAUI)

A **.NET MAUI cross-platform application** for desktop that connects **project owners** with **skill providers**.  
The app features project management, user profiles, messaging, and a contract workflow to streamline collaboration.

---

## Features

- **Project Posting & Management**  
  Project owners can create, update, and manage their projects.  

- **User Profiles**  
  Skill providers and project owners maintain profiles showcasing expertise, experience, and availability.  

- **Messaging System**  
  Real-time chat between project owners and skill providers for smooth communication.  

- **Location-Based Matching**  
  Suggests connections based on proximity for easier collaboration.  

- **Contract Workflow**  
  Structured process for negotiation, agreement, and tracking of project contracts.  

---

## Architecture & Design

- **MVVM Pattern** – Ensures a clean separation between UI, business logic, and data.  
- **Service Layers** – Centralized logic for reusable functionality.  
- **Repositories & Interfaces** – Abstraction for data access, supporting maintainability and testing.  
- **Unit of Work Pattern** – Handles multiple repository interactions in a single transaction.  
- **Dependency Injection** – Promotes modularity and testability.  

---

## Technologies Used

- **C# / .NET MAUI** – Cross-platform app development.  
- **JSON** – Data serialization and persistence.  
- **Dependency Injection** – For scalable and maintainable architecture.  

---

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/cuadravan/project-linkage-maui-desktop.git

2. Open the solution in **Visual Studio 2022** (with .NET MAUI workload installed).

3. Restore NuGet packages:

   ```bash
   dotnet restore
   ```

4. Build and run the application:

   ```bash
   dotnet build
   ```

---

## Roadmap

* [ ] Mobile platform support (iOS/Android).
* [ ] Advanced search and filtering for projects.

---

## Note
This repository was restructured and cleaned up.  
Earlier commits from the original version are not included.

