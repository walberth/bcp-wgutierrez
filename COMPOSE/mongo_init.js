db = db.getSiblingDB('Pagos');

try {
  // Drop collection if it exists (optional, for dev environments)
  db.Pagos.drop();
  print('Existing "Pagos" collection dropped (if it existed).');
} catch (err) {
  print('Warning: Could not drop collection "Pagos". It might not exist.');
}

try {
  // Create collection with schema validation
  db.createCollection('Pagos', {
    validator: {
      $jsonSchema: {
        bsonType: 'object',
        required: ['FechaPago', 'IdCliente', 'FormaPago'],
        properties: {
          FechaPago: {
            bsonType: 'date',
            description: 'Payment date'
          },
          IdCliente: {
            bsonType: 'int',
            description: 'Client ID (integer)'
          },
          IdPedido: {
            bsonType: 'int',
            description: 'Order ID (integer)'
          },
          FormaPago: {
            bsonType: 'int',
            enum: [1, 2, 3], // Only accepts 1, 2, or 3
            description: 'Payment method: 1 = Efectivo, 2 = TDC, 3 = TDD'
          },
          MontoPago: {
            bsonType: 'decimal',
            description: 'Payment amount (Decimal128)'
          }
        }
      }
    }
  });

    print('Collection "Pagos" created successfully with validation rules.');
} catch (err) {
  print('ERROR: Failed to create "Pagos" collection. Reason: ' + err.message);
  quit(1); // Stop script execution on error
}

try {
  // Insert sample data
  db.Pagos.insertMany([
    {
      FechaPago: new Date('2024-02-01'),
      IdPedido: 1,
      IdCliente: 1,
      FormaPago: 1, // Efectivo
      MontoPago: NumberDecimal("145.50")
    },
    {
      FechaPago: new Date('2024-02-02'),
      IdPedido: 2,
      IdCliente: 2,
      FormaPago: 2, // TDC
      MontoPago: NumberDecimal("200.75")
    },
    {
      FechaPago: new Date('2024-02-03'),
      IdPedido: 3,
      IdCliente: 3,
      FormaPago: 3, // TDD
      MontoPago: NumberDecimal("300.00")
    }
  ]);

  print('Sample data inserted successfully into "Pagos" collection.');
} catch (err) {
  print('ERROR: Failed to insert sample data into "Pagos". Reason: ' + err.message);
  quit(1); // Stop further execution on error
}

print('Initialization script for "Pagos" completed successfully.');