import { Component, inject, OnInit } from '@angular/core';
import { SignalrServiceService} from './signalr-service.service';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
  http = inject(HttpClient);
  constructor() {
  }

  ngOnInit(): void {
    // Initialize SignalR connection
    this.signalRService.startConnection();

    // Subscribe to real-time progress updates
    this.signalRService.listenToProgress((progress) => {
      this.progress = progress;
    });

    // Subscribe to real-time error updates
    this.signalRService.listenToError((error) => {
      this.error += error + '\n';
      console.error('Received error from server:', error);
    });
    this.signalRService.listenToFinish((message) => {
      console.log("Download finished:", message);
      // Optionally update UI to show the download is complete, for example:
      this.progress = message;
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



    // Make the POST request to the backend
    this.http
      .post(`https://localhost:7132/api/YoutubeDL/start-download?connectionId=${connectionId}`, payload)
      .subscribe({
        next: () => {
          /* this.progress = 'Download started...';*/
          console.log('Download request accepted.');
        },
        error: (err) => {
          this.error = `Failed to start download: ${err.message}`;
        },
      });
  }
}
