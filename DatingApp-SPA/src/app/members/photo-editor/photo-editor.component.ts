import { AlertifyService } from 'src/app/_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { Photo } from './../../_models/photo';
import { AuthService } from 'src/app/_services/auth.service';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {

  @Input() photos: Photo[];
  @Output() mainPhotoFromChild = new EventEmitter<string>();
  mainPhoto: Photo;
  uploader: FileUploader;
  hasBaseDropZoneOver = false;

  baseUrl = environment.apiUrl;


  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  ngOnInit() {

    this.initializeUploader();

  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }


  initializeUploader() {

    this.uploader = new FileUploader({

      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024

    });

    this.uploader.onAfterAddingFile = (file) => {file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo: Photo = {
          id: res.id,
          url: res.url,
          description: res.description,
          dateAdded: res.dateAdded,
          isMain: res.isMain
        };
        this.photos.push(photo);

        if (photo.isMain) {
          this.saveMainPhoto(photo);
        }

      }
    };


  }

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(
      () => {
        this.alertify.success('Photo has been set to Main');

        this.mainPhoto = this.photos.filter(m => m.isMain === true)[0];
        this.mainPhoto.isMain = false;
        photo.isMain = true;
        this.saveMainPhoto(photo);

      },
      err => {
        this.alertify.error(err);
      }
    );
  }

  saveMainPhoto(photo: Photo) {

    // this.mainPhotoFromChild.emit(photo.url);
    this.authService.changeMemberPhoto(photo.url);
    this.authService.currentUser.photoUrl = photo.url;
    localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
    // .......

  }

  deletePhoto(id: number) {

    this.alertify.confirm('Are you sure ?', () => {

      // ................ my approach .....................................................
      // this.userService.deletePhoto(this.authService.currentUser.id, id).subscribe(() => {
      //  this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
      //  this.alertify.warning('Photo was deleted');
      // }, err => {
      //   console.log(err);
      //   this.alertify.error('Photo was not deleted !');
      // });

      this.userService.deletePhoto(this.authService.decodedToken.nameid, id).subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
        this.alertify.success('Photo was deleted !');
      }, err => {
        console.log(err);
        this.alertify.error('Photo was not deleted !');
      });

    });

  }

}
