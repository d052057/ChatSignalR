import { Component, inject, OnInit } from '@angular/core';
import { SignalrServiceService} from './signalr-service.service';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-download',
  imports: [NgIf, FormsModule],
  templateUrl: './download.component.html',
  styleUrl: './download.component.scss'
})
export class DownloadComponent {
  url: string = 'https://www.youtube.com/watch?v=doVk4V6gYXs&t=978s';
  outputFolder: string = 'c:\\medias\\poster';  // Bind this to your UI input.
  audioOnly: boolean = false;
  progress: string = '';
  error: string = '';
  signalRService = inject(SignalrServiceService);
  constructor() {
  }

  ngOnInit(): void {
    // Initialize SignalR connection
    this.signalRService.startConnection();

    // Subscribe to progress messages
    this.signalRService.addHandler('ReceiveProgress', (progress: string) => {
      this.progress = `${progress}`;
    });

    // Subscribe to error messages
    this.signalRService.addHandler('ReceiveError', (error: string) => {
      this.error += `${error}` + "\n\n";
    });

    // Subscribe to download finished
    this.signalRService.addHandler('DownloadFinished', (message: string) => {
      this.progress = `${message}`;
    });
  }

  startDownload(): void {
    // Retrieve SignalR connection ID
    const connectionId = this.signalRService.getConnectionId();
    if (!connectionId) {
      alert('Connection to SignalR not established yet. Please wait...');
      return;
    }
    // Prepare the request payload
    const payload = {
      url: this.url,
      audioOnly: this.audioOnly,
      outputFolder: this.outputFolder  // Send the user-provided output folder.
    };
    this.signalRService.invokeMethod('HubStartDownloadServiceAsync', payload);
  }
}
