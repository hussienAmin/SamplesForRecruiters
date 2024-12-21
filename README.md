
# Introduction  
Welcome to my portfolio of code samples, demonstrating my expertise in software development.  



# Technologies

- [Backend](#backend)
- [WPF](#wpf)
- [MAUI](#maui)
- MAUI for mobile and desktop applications  
- SQL Server  

## Backend
- [Entity Framework](#entity-framework)
### Entity Framework
## WPF
## MAUI
## Structure  
1. **Web APIs**  
   - [Project Name] - Highlights best practices in API development with .NET Core.  
2. **Desktop Applications**  
   - [Project Name] - Showcases WPF with MVVM and dependency injection.  
3. **Mobile Applications**  
   - [Project Name] - Demonstrates mobile-first solutions using MAUI.

# WPF with MVVM and Dependency Injection  

This project demonstrates a WPF application using the MVVM design pattern and Dependency Injection (DI) for decoupled, testable, and maintainable code.  

## Key Features  
- MVVM for separation of concerns.  
- Dependency Injection with [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).  
- Asynchronous data operations using `IAsyncRelayCommand` from Community Toolkit MVVM.  
- Modern UI design principles.  

## Technologies  
- **WPF**  
- **MVVM Pattern**  
- **Dependency Injection**  
- **Community Toolkit MVVM**  
- **Entity Framework Core**  

## Application Flow  
1. **Login Screen**  
   - Validates user credentials.  
   - Uses DI to inject authentication services.  
2. **Dashboard**  
   - Displays user-specific data fetched from a repository.  
   - Commands and bindings demonstrate MVVM principles.  
3. **Settings**  
   - Save user preferences using a service injected into the ViewModel.
# **Bonus**  
*Bonus is a comprehensive system for managing sales, inventory, and employee operations , e-invoice integration, and product showcasing.*

![logo](https://raw.githubusercontent.com/hussienAmin/SamplesForRecruiters/refs/heads/main/1734443942560.jpg)
## **Features**  
- **Sales Management**:  
  Manage sales operations, including basket processing and invoice generation.  
- **Inventory Management**:  
  Keep track of products, deliveries, and store activities.  
- **Employee Operations**:  
  Handle salaries, advances, fines, and incentives with notifications for salary disbursement.  
- **Product Showcasing**:  
  Showcase products online, allowing customers to place orders and track fulfillment.  
- **News Module**:  
  Subscribe to Kaf News for analysis and content across various fields.  
- **E-Invoice Integration**:  
  Supports the Egyptian Government E-Invoice system.  

## **System Structure**  
KafApp includes the following instances:  
1. **API**  
2. **Desktop Admin Instance**  
3. **Desktop User Instance**  
4. **Desktop POS Instance**  
5. **Desktop Store Instance**  
6. **Android User Instance**  
7. **Android POS Instance**  
8. **Android Store Instance**  
9. **Android Client Instance**  

## **Technologies Used**  
- **Backend**: ASP.NET Core 8, Entity Framework Core  
- **Frontend**: WPF, MAUI, Android  
- **Database**: SQL Server  
- **Other Tools**: Community Toolkit MVVM  

## **How to Use**  
1. Clone this repository:  
   ```bash
   git clone https://github.com/YourRepo/KafApp.git
