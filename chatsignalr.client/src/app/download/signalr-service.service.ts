import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
@Injectable({
  providedIn: 'root'
})
export class SignalrServiceService {
  private hubConnection!: signalR.HubConnection;
  private connectionId: string = '';
  constructor() {
  }

  getConnectionId(): string {
    return this.connectionId;
  }

  startConnection(): void {
    /*const hubUrl = `${window.location.origin}/downloadHub`;*/
    const hubUrl = "https://localhost:7132/downloadHub";
    // Initialize the SignalR connection
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { withCredentials: true }) // Update URL if needed
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Start the connection and log results
    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connected');
        // Capture the connection ID from the server
        this.hubConnection.invoke('GetConnectionId')
          .then((id: string) => {
            this.connectionId = id;
            console.log(`Connection ID retrieved: ${this.connectionId}`);
          })
          .catch(err => console.error('Error retrieving Connection ID:', err));
      })
      .catch(err => {
        console.error('Error while starting SignalR connection:', err);
        setTimeout(
          () => this.startConnection()
          , 2000); // Retry after 2 seconds
      });

    // Debugging: Log connection lifecycle events
    this.hubConnection.onclose((error) => {
      console.error('SignalR connection closed:', error);
    });

    this.hubConnection.onreconnecting((error) => {
      console.warn('SignalR reconnecting:', error);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected successfully.');
    });

    // Periodic connection state logging
    setInterval(() => {
      console.log(`SignalR connection state: ${this.hubConnection.state}`);
    }, 1000);
  }

  listenToProgress(callback: (progress: string) => void): void {
    this.hubConnection.on('ReceiveProgress', (progress: string) => {
      callback(progress);
    });
  }

  listenToError(callback: (error: string) => void): void {
    this.hubConnection.on('ReceiveError', (error: string) => {
      callback(error);
    });
  }

  listenToFinish(callback: (finish: string) => void): void {
    this.hubConnection.on('DownloadFinished', (message: string) => {
      callback(message);
    });
  }

  //async invokeMethod(methodName: string, ...args: any[]): Promise<void> {
  //  try {
  //        alert("invokeMethod: " + methodName);
  //        return await this.hubConnection.invoke(methodName, ...args);
  //    } catch (err) {
  //        console.error(`Error invoking method '${methodName}':`, err);
  //        throw err;
  //    }
  //}

  //addHandler(eventName: string, callback: (...args: any[]) => void): void {
  //  this.hubConnection.on(eventName, callback);
  //  console.log(`Handler added for event: ${eventName}`);
  //}
}
