// Singleton to be reused outside React
import * as signalR from '@microsoft/signalr';

/**
 * Manages all incoming and outgoing communication with the SignalR Hub
*/
class SignalRManager {  // this is a SINGLETON class! becasue i don't want to accidentially create another SignalR connection
    private static instance: SignalRManager;
    protected connection: signalR.HubConnection;
    private isConnected: boolean = false;
    private readyCb: (() => void)[] = []; // list of callback functions waiting for onReady

    constructor(sector: string) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${window.location.origin}/api/hub?sector=${sector}`, {
                withCredentials: false
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Trace)
            .build()
    }

    /**
     * Gets a SignalRManager instance and stores it
     * @returns the SignalRManager instance
     */
    public static createInstance(sector: string): SignalRManager {
        SignalRManager.instance = new SignalRManager(sector);
        return SignalRManager.instance;
    }

    /**
     * Gets the SignalRManager instance if it exists
     * @returns the SignalRManager instance
     */
    public static getInstance(): SignalRManager {
        return SignalRManager.instance;
    }

    /**
     * Called when the a subscriber wants a method to execute once the SignalR connection has been established
     * @param callback a callback function
     */
    public onReady(callback: (() => void)) {
        if (this.isConnected) {
            callback();
        } else {
            this.readyCb.push(callback);
        }
    }

    /**
     * Starts a new SignalR connection
     */
    public async startConnection() {
        await this.connection.start()
            .then(() => {
                console.log('SignalR connection started');
                this.isConnected = true;
                this.readyCb.forEach((cb: (() => void)) => cb());
                this.readyCb = [];
            })
            .catch((err: any) => console.error('SignalR connection error: ', err));
    }

    /**
     * Stops the SignalR connection
     */
    public async stopConnection() {
        await this.connection.stop()
            .then(() => {
                console.log('SignalR connection disconnected')
                this.isConnected = false;
            })
            .catch((err: any) => console.error('SignalR disconnection error: ', err));
    }

    /**
     * Called when a new SignalR message is recieved
     * Adds a handler
     * @param eventName name of the function
     * @param callback parameters of the function
     */
    public on(eventName: string, callback: (...params: any[]) => void) {
        this.connection.on(eventName, callback);
    }

    /**
     * Called when a new SignalR message is recieved
     * Removes the handler
     * @param eventName name of the function
     * @param callback parameters of the function
     */
    public off(eventName: string, callback: (...params: any[]) => void) {
        this.connection.off(eventName, callback);
    }

    /**
     * Sends a new message to the SignalR server
     * @param methodName name of the function
     * @param data parameters of the function
     */
    public send(methodName: string, ...data: any[]) {
        return this.connection.invoke(methodName, ...data);
    }
}

export default SignalRManager