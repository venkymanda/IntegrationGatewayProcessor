# Integration Gateway Processor

![GitHub repo size](https://img.shields.io/github/repo-size/venkymanda/IntegrationGatewayProcessor)
![GitHub stars](https://img.shields.io/github/stars/venkymanda/IntegrationGatewayProcessor)
![GitHub forks](https://img.shields.io/github/forks/venkymanda/IntegrationGatewayProcessor)
![GitHub issues](https://img.shields.io/github/issues/venkymanda/IntegrationGatewayProcessor)
![GitHub license](https://img.shields.io/github/license/venkymanda/IntegrationGatewayProcessor)

The Integration Gateway Processor is a component built using Azure Durable Functions that serves as an intermediary for receiving and sending files for the Integration Gateway Service. This repository contains the source code and documentation for the processor.

## Table of Contents

- [Introduction](#introduction)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Introduction

Managing the flow of files between the Integration Gateway Service and other systems can be complex. The Integration Gateway Processor simplifies this process by leveraging Azure Durable Functions to orchestrate the sending and receiving of files securely.

Key features of the Integration Gateway Processor:
- Integration with Azure Durable Functions for reliable file processing.
- Support for receiving and sending files in various formats.
- Configurable options to customize the processing workflow.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- Azure account with access to Azure Durable Functions.
- .NET Core SDK installed on your development machine.

## Getting Started

To get started with the Integration Gateway Processor, follow these steps:

1. Clone this repository to your local machine:

   ```bash
   git clone https://github.com/venkymanda/IntegrationGatewayProcessor.git

2. Build the project using Visual Studio or the .NET CLI.

3. Configure the processor by editing the local.settings.json file with your Azure Durable Functions settings and processing configuration.

4. Deploy the Azure Durable Functions project to your Azure environment.

5. Create and manage orchestrations and activities based on your specific file processing needs.

## Usage

The Integration Gateway Processor acts as a central component for receiving and sending files. You can trigger it to process files by initiating orchestrations using Azure Durable Functions. Monitor the processor's logs and orchestrations for details on file processing.

## Contributing

Contributions are welcome! If you have any improvements or feature requests, please submit an issue or a pull request. We appreciate your help in making this processor better.

## LICENSE

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details