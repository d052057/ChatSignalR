import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  imports: [RouterOutlet, RouterLink],
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'chatsignalr.client';
}
