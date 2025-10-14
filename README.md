# Introduction 
This repository contains the demo code for the presentation "What is .NET Aspire and why should you use it?"

## Abstract
.NET Aspire is a comprehensive set of tools, templates, and packages to help you build observable, production-ready, distributed applications. It simplifies common challenges in modern app development by focusing on enhancing the local development experience. It provides abstractions that streamline the setup of your app's configuration, service discovery, environment variables, and container configurations. Aspire makes it easy to define everything your app needs using integrations designed to simplify connections to popular services and platforms.
This presentation goes through the basic of why and how of .NET Aspire. And hopefully you will leave with new ideas and inspiration of how you can use .NET Aspire in your projects.

# The Demo
Is based on the Aspire-starter template. To create your own version just run the following command:

```bash
dotnet new aspire-starter -n MyAspireApp
```

This will create a new solution with an API and a Blazor Web application.

# Running the Demo
To run the demo, you need to have the .NET SDK installed. You can download it from [the official .NET website](https://dotnet.microsoft.com/download).
We also recommend using Aspire CLI for managing your Aspire projects. You can install it using the following command:

```bash
dotnet tool install -g Aspire.Cli
```
After installing the CLI, you can run the demo by executing the following command in the root directory of the project:

```bash
aspire run
```

