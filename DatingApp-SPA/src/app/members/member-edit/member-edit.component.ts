import { UserService } from './../../_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { NgForm } from '@angular/forms';
import { AuthService } from 'src/app/_services/auth.service';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {

  user: User;
  photoUrl: string;

  @ViewChild('editForm') editFormVariable: NgForm;
  @HostListener('window: beforeunload', ['$event'])
  unloadNotification($event: any) {

    if (this.editFormVariable.dirty) {

      $event.returnValue = true;

    }
  }

  // tslint:disable-next-line:max-line-length
  constructor( private route: ActivatedRoute, private alertify: AlertifyService, private userService: UserService, private authService: AuthService ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      console.warn('|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||');
      console.warn(data);
      console.warn('|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||');
      this.user = data['user'];
    });

    this.authService.currentPhotoUrl.subscribe(url => this.photoUrl = url);
  }


  updateUser() {

    console.log(this.user);

    this.userService.updateUser(this.authService.decodedToken.nameid, this.user).subscribe( next => {

      this.alertify.success('User was updated successfuly !');
      this.editFormVariable.reset(this.user);

    }, err => {
      this.alertify.error(err);
    });

  }

  setMianPhoto(mainPhotoUrl) {
    this.user.photoUrl = mainPhotoUrl;
  }


}
