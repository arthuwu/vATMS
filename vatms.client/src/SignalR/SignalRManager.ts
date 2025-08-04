// Singleton to be reused outside React
import * as signalR from '@microsoft/signalr';

// TODO: cleanup

/*

off = (eventName) => {
  this.connection.off(eventName);
};


// component:

const signalR = useSignalR();

useEffect(() => {
  const handleMessage = (msg) => {
    console.log("Received:", msg);
  };

  signalR.on("ReceiveMessage", handleMessage);

  return () => {
    signalR.off("ReceiveMessage", handleMessage);  // Cleanup handler
  };
}, []);

const sendMessage = () => {
    signalR.send("SendMessage", "Hello from React!");
  };
*/

class SignalRManager {  // this is a SINGLETON class! becasue i don't want to accidentially create another SignalR connection
    private static instance: SignalRManager;
    protected connection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            // http://localhost:5239 for dotnet
            .withUrl(`${window.location.origin}/api/hub`, { // TODO: find a way to make this persist without specifying port
                withCredentials: false
            })
            .withAutomaticReconnect()
            .build()
    }

    public static getInstance(): SignalRManager { 
        if (!SignalRManager.instance) {
            SignalRManager.instance = new SignalRManager();
        }
        return SignalRManager.instance;
    }

    public async startConnection() {
        await this.connection.start()
            .then(() => console.log('SignalR connection started'))
            .catch((err: any) => console.error('SignalR connection error: ', err));
    }

    public async stopConnection() {
        await this.connection.stop()
            .then(() => console.log('SignalR connection disconnected'))
            .catch((err: any) => console.error('SignalR disconnection error: ', err));
    }

    public on(eventName: string, callback: () => void) {
        this.connection.on(eventName, callback);
    }

    public send(methodName: string, data: any) {
        return this.connection.invoke(methodName, data);
    }
}

export default SignalRManager