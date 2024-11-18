# Saga Orchestration Project

It integrates with **MassTransit** and **RabbitMQ** to provide reliable transaction coordination in distributed systems.

## Features

**Saga State Machine**:
  - Manages the status of stock orders (Pending, Approved, Rejected) using a state machine.
  - State transitions are triggered asynchronously via RabbitMQ messages.
 
**Order Processing**:
  - Receives the order and forwards it to the stock service for validation.
  - Ensures the order flows seamlessly into the stock validation phase.

**Stock Processing**:
  - Verifies stock availability before proceeding to payment.
  - If stock is insufficient, the process terminates and marks the order as **Rejected**.
  - If stock is sufficient, the order proceeds to the payment step.
    
**Payment Processing**:
  - If payment succeeds, the order is approved; otherwise, it is rejected.

**MassTransit for Saga Management**:
  - Ensures reliable coordination between order and payment processes.
  - Utilizes **RabbitMQ** as the messaging system.

## Technologies Used

- **MassTransit**: For saga orchestration and messaging.
- **RabbitMQ**: For inter-service messaging in distributed systems.
- **.NET 8**: For developing the project.
- **MongoDB**: For database operations of the Stock service.
- **MSSQL**: For database operations of the Order service and Masstransit.

