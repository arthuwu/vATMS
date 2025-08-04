import React, { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import './App.css';

interface Message {
    username: string;
    text: string;
}

function App() {
    const [messages, setMessages] = useState<Message[]>([]);
    const [currentMessage, setCurrentMessage] = useState<string>('');
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    const username = useRef<string>(new Date().getTime().toString()).current;

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            // http://localhost:5239 for dotnet
            .withUrl(`${window.location.origin}/api/hub`, { // TODO: find a way to make this persist without specifying port
                withCredentials: false
            })
            .withAutomaticReconnect()
            .build()

        connectionRef.current = connection;

        connection.on('MessageReceived', (username: string, message: string) => {
            const newMessage: Message = { username, text: message };
            setMessages(prevMessages => [...prevMessages, newMessage]);
        });

        connection.start()
            .then(() => console.log('SignalR Connection started'))
            .catch((err: any) => console.error('SignalR Connection error: ', err));

        return () => {
            connection.stop().then(() => console.log('SignalR Connection stopped'));
        }
    }, [messages])

    const sendMessage = async () => {
        if (connectionRef.current && currentMessage.trim() != "") {
            try {
                await connectionRef.current.send("NewMessage", username, currentMessage);
                setCurrentMessage("");
            } catch (e) {
                console.error("Send failed:", e);
            }
        }
    }

    const handleKeyUp = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            sendMessage();
        }
    }
    return (
        <>
            <div className="messages" id="divMessages">
                {messages.map((msg, index) => (
                    <div key={index} className="message">
                        <div className="message-author">{msg.username}</div>
                        <div>{msg.text}</div>
                    </div>
                ))}
                <div ref={messagesEndRef} />
            </div>
            <div className="input-zone">
                <label htmlFor="tbMessage">Message:</label>
                <input
                    id="tbMessage"
                    className="input-zone-input"
                    type="text"
                    value={currentMessage}
                    onChange={e => setCurrentMessage(e.target.value)}
                    onKeyUp={handleKeyUp}
                />
                <button id="btnSend" onClick={sendMessage}>
                    Send
                </button>
            </div>
        </>
    );
}

export default App;