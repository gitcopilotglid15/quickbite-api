# QuickBite API

A simple REST API for a food ordering system.

## Features

- User management
- Menu item management
- Order creation and tracking
- RESTful endpoints
- JSON responses

## Installation

1. Clone the repository
2. Install dependencies:
   ```bash
   npm install
   ```

## Usage

Start the server:
```bash
npm start
```

For development with auto-reload:
```bash
npm run dev
```

The API will be available at `http://localhost:3000`

## API Endpoints

### Root
- `GET /` - API information and available endpoints

### Users
- `GET /users` - Get all users
- `POST /users` - Create a new user
  ```json
  {
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890"
  }
  ```

### Menu
- `GET /menu` - Get all menu items
- `POST /menu` - Add a new menu item
  ```json
  {
    "name": "Burger",
    "description": "Delicious beef burger",
    "price": 12.99,
    "category": "Main Course"
  }
  ```

### Orders
- `GET /orders` - Get all orders
- `GET /orders/:id` - Get a specific order
- `POST /orders` - Create a new order
  ```json
  {
    "userId": "user-uuid",
    "items": [
      {
        "menuItemId": "menu-item-uuid",
        "quantity": 2
      }
    ]
  }
  ```

## Response Format

All responses are in JSON format. Error responses include an `error` field with a descriptive message.

## Data Storage

This implementation uses in-memory storage for simplicity. In a production environment, you would integrate with a database.