db = db.getSiblingDB('Pagos'); // Replace with your database name

db.createCollection('Pagos');

db.Pagos.insertMany([
  { IdCliente: 1, IdPedido: 1, MontoPago: 145, FormaPago: 'Tarjeta de Crédito' },
  { IdCliente: 2, IdPedido: 2, MontoPago: 145, FormaPago: 'PayPal' },
  { IdCliente: 3, IdPedido: 3, MontoPago: 145, FormaPago: 'Transferencia Bancaria' }
]);
