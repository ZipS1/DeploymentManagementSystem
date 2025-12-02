> This is my bachelor's thesis project, and I poured countless hours, late nights, and a lot of heart into it. I've since graduated and moved forward, so this repository is now archived. It remains here as a snapshot of my growth as a developer and a reminder of a chapter I'm really proud of ✨
---
# ProjectOps - IT Project Management System with CI/CD Automation

## Project Description

**ProjectOps** is a modern IT project management system that combines classic task management with CI/CD process automation. The key feature of the system is the ability to automatically trigger deployments through task status changes, eliminating the gap between PM and DevOps teams.

The system is designed for development teams that require tight integration between project management and continuous integration and deployment processes. ProjectOps solves the problem of tool fragmentation, where PMs don't control deployments and DevOps spend time manually triggering pipelines.

## Setup Instructions

### Prerequisites

- **Docker** and **Docker Compose** installed on the system
- **Git** for repository cloning

### Quick Start with Docker

1. **Clone the repository**:
   ```bash
   git clone https://github.com/ZipS1/DeploymentManagementSystem.git
   cd DeploymentManagementSystem
   ```

2. **Set up environment variables**:
   ```bash
   # Create .env file in the project root
   echo "PG_PASSWORD=your_secure_password" > .env
   ```

3. **Launch with Docker Compose**:
   ```bash
   docker-compose up -d
   ```

4. **Access the application**:
   - Open your browser and navigate to `http://localhost:80`
   - Database will be available on port `5432`

## Technology Stack

### Core Technologies
- **Framework**: ASP.NET Core 8.0
- **Architecture**: Blazor Server
- **Programming Language**: C# (.NET 8.0)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core with Npgsql provider

### Infrastructure and Deployment
- **Containerization**: Docker, Docker Compose
- **CI/CD**: GitLab CI/CD
- **Web Server**: Kestrel (built into ASP.NET Core)
- **Authentication**: ASP.NET Core Identity
- **User Interface**: Blazor Components
- **Communication**: SignalR (for Blazor Server)
- **Static Files**: wwwroot (CSS, JavaScript, images)

## Project Structure

```
DeploymentManagementSystem/
├── Components/                    # Blazor components
│   └── Pages/                    # Application pages
│       ├── ProjectPages/         # Project management pages
│       ├── TaskPages/            # Task management pages
│       ├── WorkLogPages/         # Work log pages
│       ├── Home.razor            # Home page
│       └── UserList.razor        # User list
├── Data/                         # Data models and DB context
│   ├── Models/                   # Domain models
│   │   ├── Project.cs           # Project model
│   │   ├── Task.cs              # Task model
│   │   ├── Environment.cs       # Environment model
│   │   ├── TaskStatus.cs        # Task status model
│   │   ├── TaskType.cs          # Task type model
│   │   └── WorkLog.cs           # Work log model
│   ├── Configurations/          # Entity Framework configurations
│   ├── ApplicationDbContext.cs  # Database context
│   └── ApplicationUser.cs       # User model
├── Services/                     # Business logic and services
│   ├── GitlabService.cs         # GitLab integration service
│   ├── DeploymentService.cs     # Deployment management service
│   └── DeploymentQueue.cs       # Deployment queue
├── Extensions/                   # Extensions and helper methods
├── Localization/                 # Localization files
├── Properties/                   # Project properties
├── wwwroot/                      # Static files
├── Program.cs                    # Application entry point
├── Dockerfile                    # Docker image configuration
├── docker-compose.yml            # Development configuration
├── deploy.docker-compose.yml     # Production configuration
├── .gitlab-ci.yml               # CI/CD configuration
├── appsettings.json             # Application settings
└── DeploymentManagementSystem.csproj # Project file
```

## Functionality

### Core Features
- **Project Management**: Create, edit, and delete projects with Git repository binding
- **Task Management**: Support for three task types — Analysis, Feature Enhancement, Bug Fix
- **Role System**: Four user roles with different access levels
- **GitLab Integration**: Automatic branch creation, MR creation, and CI/CD pipeline triggering
- **Automatic Deployment**: Deployment trigger on task status change
- **Work Time Tracking**: Ability to add work time to tasks

### User Roles
- **Administrator**: Full system access, user management
- **Project Manager (PM)**: Project creation, team management, deployment control
- **Lead Developer (Lead)**: Task management, production deployment
- **Developer**: Task work, time tracking
