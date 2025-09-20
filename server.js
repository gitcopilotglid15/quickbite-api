const express = require('express');
const cors = require('cors');
const { v4: uuidv4 } = require('uuid');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());

// In-memory storage (for simplicity)
let users = [];
let menuItems = [];
let orders = [];

// Root endpoint
app.get('/', (req, res) => {
  res.json({ 
    message: 'Welcome to QuickBite API',
    version: '1.0.0',
    endpoints: {
      users: '/users',
      menu: '/menu',
      orders: '/orders'
    }
  });
});

// User endpoints
app.get('/users', (req, res) => {
  res.json(users);
});

app.post('/users', (req, res) => {
  const { name, email, phone } = req.body;
  
  if (!name || !email) {
    return res.status(400).json({ error: 'Name and email are required' });
  }

  const user = {
    id: uuidv4(),
    name,
    email,
    phone: phone || null,
    createdAt: new Date().toISOString()
  };

  users.push(user);
  res.status(201).json(user);
});

// Menu endpoints
app.get('/menu', (req, res) => {
  res.json(menuItems);
});

app.post('/menu', (req, res) => {
  const { name, description, price, category } = req.body;
  
  if (!name || !price) {
    return res.status(400).json({ error: 'Name and price are required' });
  }

  const menuItem = {
    id: uuidv4(),
    name,
    description: description || '',
    price: parseFloat(price),
    category: category || 'Other',
    available: true,
    createdAt: new Date().toISOString()
  };

  menuItems.push(menuItem);
  res.status(201).json(menuItem);
});

// Order endpoints
app.get('/orders', (req, res) => {
  res.json(orders);
});

app.get('/orders/:id', (req, res) => {
  const order = orders.find(o => o.id === req.params.id);
  if (!order) {
    return res.status(404).json({ error: 'Order not found' });
  }
  res.json(order);
});

app.post('/orders', (req, res) => {
  const { userId, items } = req.body;
  
  if (!userId || !items || !Array.isArray(items) || items.length === 0) {
    return res.status(400).json({ error: 'UserId and items array are required' });
  }

  // Validate user exists
  const user = users.find(u => u.id === userId);
  if (!user) {
    return res.status(400).json({ error: 'User not found' });
  }

  // Calculate total and validate menu items
  let total = 0;
  const orderItems = [];

  for (const item of items) {
    const menuItem = menuItems.find(m => m.id === item.menuItemId);
    if (!menuItem) {
      return res.status(400).json({ error: `Menu item ${item.menuItemId} not found` });
    }
    
    const quantity = item.quantity || 1;
    total += menuItem.price * quantity;
    
    orderItems.push({
      menuItemId: menuItem.id,
      name: menuItem.name,
      price: menuItem.price,
      quantity
    });
  }

  const order = {
    id: uuidv4(),
    userId,
    userName: user.name,
    items: orderItems,
    total: Math.round(total * 100) / 100, // Round to 2 decimal places
    status: 'pending',
    createdAt: new Date().toISOString()
  };

  orders.push(order);
  res.status(201).json(order);
});

// Error handling middleware
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).json({ error: 'Something went wrong!' });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({ error: 'Endpoint not found' });
});

app.listen(PORT, () => {
  console.log(`QuickBite API server running on port ${PORT}`);
});

module.exports = app;