db = db.getSiblingDB("Consultas");

try {
  // Drop collection if it exists (optional, for dev environments)
  db.Consultas.drop();
  print('Existing "Consultas" collection dropped (if it existed).');
} catch (err) {
  print('Warning: Could not drop collection "Consultas". It might not exist.');
}

try {
  // Create collection with schema validation
  db.createCollection("Consultas", {
    validator: {
      $jsonSchema: {
        bsonType: "object",
        required: [
          "IdPedido",
          "NombreCliente",
          "IdPago",
          "FormaPago",
          "MontoPago",
        ],
        properties: {
          IdPedido: {
            bsonType: "int",
            description: "Order ID (integer)",
          },
          NombreCliente: {
            bsonType: "string",
            description: "Client Name ID (string)",
          },
          IdPago: {
            bsonType: "string",
            description: "Payment ID (string)",
          },
          FormaPago: {
            bsonType: "int",
            enum: [1, 2, 3], // Only accepts 1, 2, or 3
            description: "Payment method: 1 = Efectivo, 2 = TDC, 3 = TDD",
          },
          MontoPago: {
            bsonType: "decimal",
            description: "Payment amount (Decimal128)",
          },
        },
      },
    },
  });

  print('Collection "Consultas" created successfully with validation rules.');
} catch (err) {
  print(
    'ERROR: Failed to create "Consultas" collection. Reason: ' + err.message
  );
  quit(1); // Stop script execution on error
}

try {
  // Insert sample data
  db.Consultas.insertMany([
    {
      IdPedido: 1,
      NombreCliente: "WALBERTH",
      IdPago: "67b38489908dbc8340544ca7",
      FormaPago: 1, // Efectivo
      MontoPago: NumberDecimal("145.50"),
    },
    {
      IdPedido: 2,
      NombreCliente: "ANGELA",
      IdPago: "67b38489908dbc8340544ca8",
      FormaPago: 2, // TDC
      MontoPago: NumberDecimal("200.75"),
    },
    {
      IdPedido: 3,
      NombreCliente: "FELIPE",
      IdPago: "67b38489908dbc8340544ca9",
      FormaPago: 3, // TDD
      MontoPago: NumberDecimal("300.00"),
    },
  ]);

  print('Sample data inserted successfully into "Consultas" collection.');
} catch (err) {
  print(
    'ERROR: Failed to insert sample data into "Consultas". Reason: ' + err.message
  );
  quit(1); // Stop further execution on error
}

print('Initialization script for "Consultas" completed successfully.');
