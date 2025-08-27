mergeInto(LibraryManager.library, {
    ConnectToMQTT: function(brokerUrl, port, clientId, topic) {
        var host = UTF8ToString(brokerUrl); // Should be "localhost" from C#
        var clientIdStr = UTF8ToString(clientId);
        var topicStr = UTF8ToString(topic);

        // Construct WebSocket URL with path for local host (use ws:// if no SSL)
        var url = "ws://" + host + ":" + Number(port) + "/mqtt"; // Changed to ws://
        window.mqttClient = new Paho.MQTT.Client(url, clientIdStr);

        window.mqttClient.onConnectionLost = function(responseObject) {
            var errorMsg = responseObject.errorMessage || "Connection lost";
            console.error("MQTT connection lost: " + errorMsg);
            SendMessage('HeartRateReceiver', 'OnConnectionLost', errorMsg);
        };

        window.mqttClient.onMessageArrived = function(message) {
            console.log("Received MQTT message: " + message.payloadString);
            SendMessage('HeartRateReceiver', 'OnMessageReceivedJS', message.payloadString || "");
        };

        window.mqttClient.connect({
            useSSL: false, // Changed to false to match ws://
            onSuccess: function() {
                console.log("Connected to MQTT broker at " + url);
                window.mqttClient.subscribe(topicStr, {
                    onSuccess: function() {
                        SendMessage('HeartRateReceiver', 'OnConnected');
                    },
                    onFailure: function(err) {
                        console.error("Subscription failed: " + err.errorMessage);
                        SendMessage('HeartRateReceiver', 'OnConnectionFailed', "Subscription failed: " + err.errorMessage);
                    }
                });
            },
            onFailure: function(err) {
                var errorMsg = err.errorMessage || "Connection failed (check broker settings)";
                console.error("MQTT connection failed: " + errorMsg);
                SendMessage('HeartRateReceiver', 'OnConnectionFailed', errorMsg);
            }
        });
    },

    DisconnectMQTT: function() {
        if (window.mqttClient && window.mqttClient.isConnected()) {
            window.mqttClient.disconnect();
            console.log("Disconnected from MQTT broker");
            window.mqttClient = null;
        }
    }
});