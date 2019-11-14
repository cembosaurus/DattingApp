import { AuthService } from 'src/app/_services/auth.service';
import { Message } from './../../_models/message';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {

  today: number = Date.now();

  @Input() recipientId: number;
  messages: Message[];

  constructor(private userService: UserService, private authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    this.userService.getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .subscribe(messages => {
        this.messages = messages;
      }, err => {
        this.alertify.error(err);
      });
  }

}
