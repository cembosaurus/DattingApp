import { AlertifyService } from './../../_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { AuthService } from './../../_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { Router, ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from 'ngx-gallery';


@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.css']
})
export class MemberDetailsComponent implements OnInit {

  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  constructor(private userService: UserService, private route: ActivatedRoute, private alertify: AlertifyService) { }

  ngOnInit() {

    // this.loadUser();
    // ...OR resolver ... no need for loadUser():
    this.route.data.subscribe(data => {
      this.user = data['user'];

      // ... just to get photo from local disc ...
      if (this.user.userName === 'user') {
        this.alertify.success(this.user.userName);
        this.user.photoUrl = 'C:/temp/user.jpg';
      } else if (this.user.userName === 'misurka') {
        this.alertify.success(this.user.userName);
        this.user.photoUrl = 'C:/temp/misurka.jpg';
      }

    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

    this.galleryImages = this.getImages();

  }

  // loadUser() {
  //   this.userService.getUser(+this.route.snapshot.params['id']).subscribe((userResult: User) => {
  //     this.user = userResult;
  //   }, err => {
  //     this.alertify.error(err);
  //   });
  // }

  getImages() {

  const imageUrls = [];

  for (let i = 0; i < this.user.photos.length; i++) {

    imageUrls.push(
      {
        small: this.user.photos[i].url,
        medium: this.user.photos[i].url,
        big: this.user.photos[i].url,
        description: this.user.photos[i].description,
      }
    );
  }

    return imageUrls;
  }

}
