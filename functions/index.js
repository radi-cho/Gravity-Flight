const functions = require("firebase-functions");

exports.checkInternetConnectivity = functions.https.onRequest((req, res) => {
  res.send("gravity-flight-success");
});
