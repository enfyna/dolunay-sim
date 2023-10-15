import time
import datetime
import logging
from twisted.internet import reactor
from twisted.internet.protocol import ServerFactory, Protocol
import json
import test_device

# Defines the time between server reports
SERVERSTATCALLBACK = 3600

# Client class handler
class ClientHandler(Protocol):

    def __init__(self):
        self.userid = None # Player database id, if any
        self.username = None # Player username
        self.playerCode = 0 # Players incremented user id
        self.address = None # Players ip address
        
    # Client connection has been made.
    def connectionMade(self):
        self.address = self.transport.getPeer().host
        self.server = self.factory
        self.server.addClient(self)
        print("\r\nConnection from " + self.address + " on port " + str(self.transport.getPeer().port))
        # self.sendData("Hello there, and welcome stranger")
        
    # Sends the client data. This is very basic, but works for our purpose.
    def dataReceived(self, data):
        self.protocolHandler(data)
        
    # Handles client losing connection. If you were storing any user stats, this
    # may be a good time to write their changes if you haven't done so already.
    def connectionLost(self, status):
        print("\t Disconnection from " + self.address)
        
        if not self.server.disconnectPlayer(self.username):
            print("Tried to remove a player but couldn't...")
            pass
	
    # Parses data sent from the client. Depending on how you want to send data,
    # you will need to change this yourself. But this is where client data is
    # received.
    buffer : str = ''
    def protocolHandler(self, data):
        data = data.decode('ASCII')
        self.buffer += data
        try:
            data : dict = json.loads(self.buffer)
            self.buffer = ''
            inp = test_device.main(data)

            req : dict = {
                "inputs":inp,
                "state":0
            }

            self.sendData(json.dumps(req))
        except:
            pass

    # Send data to the client
    def sendData(self, data):
        # print(f"Sending to client: {data}")
        self.transport.write(data.encode('ASCII'))
           
           
# Main server handler object
class TCPServerFactory(ServerFactory):
    protocol = ClientHandler
    
    def __init__(self):
        self.clients = [] # Clients currently connected to the server
        self.clientCount = 0 # Number of connected clients
        self.lastPlayerCode = 0 # Last player code used
        
        # Make sure the server records statistics to a log every hour
        # 3600 = 1 hour
        # 1800 = half hour
        #reactor.callLater(SERVERSTATCALLBACK, self.getServerStats)
        
    # Loop through the clients and send them each a message. This would be
    # useful if you implemented a chat or server wide messages in your game.
    def sendAll(self, data):
        for client in self.clients:
            client.sendData(data)
    
    # Adds a new client to our list
    def addClient(self, client):        
        self.clients.append(client)
            
    # Disconnects a player based on their username, maybe you want to do
    # a chat command such as /kick 'username', this would do it :D
    # Mostly tho, this is just a failsafe when a client loses connection,
    # we make sure to remove them from the client list of the server
    def disconnectPlayer(self, username, data = None):
        found = False

        for client in self.clients:
            if client.username == username:
                client.transport.loseConnection()
                found = True
                break
        return found          
            
    # Increments our player code
    def generatePlayerCode(self):
        self.lastPlayerCode += 1
        return self.lastPlayerCode
        
    # Returns a count of connected players
    def getConnectedPlayerCount(self):
        return len(self.clients)

	
if __name__ == "__main__":
    print("Server started at " + str(time.strftime("%A %B %d, %Y @ %I:%M:%S %p", time.localtime())))
    server = TCPServerFactory()
    # We prob wanna open more ports later on, but its ok for now
    reactor.listenTCP(6060, server)
    reactor.run()