import { AlertifyService } from './../_services/alertify.service';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { formControlBinding } from '@angular/forms/src/directives/ng_model';
import { fromEventPattern } from 'rxjs';
import { BsDatepickerModule, BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  user: User;
  @Input() valuesFromHome: any;
  @Output() cancelEventFromRegister = new EventEmitter();
  registerForm: FormGroup;
  custom_bsConfig: Partial<BsDatepickerConfig>;

  constructor(private authService: AuthService, private alertify: AlertifyService, private fb: FormBuilder, private router: Router) { }

  ngOnInit() {

    this.custom_bsConfig = {
      containerClass: 'theme-blue'
    };


    // ... Instantiating reactive form:
    // this.registerForm = new FormGroup({
    //   userName: new FormControl('', Validators.required),
    //   password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
    //   confirmPassword: new FormControl('', Validators.required)
    // }, this.passwordMatchValidator);

    // ... OR Factoring reactive form with builder:
    this.registerForm = this.fb.group({
      gender: ['male'],
      userName: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});

  }

  passwordMatchValidator(form: FormGroup) {
    return form.get('password').value === form.get('confirmPassword').value ? null : {'mismatch': true};
  }

  register() {

    if (this.registerForm.valid) {

      this.user = Object.assign({}, this.registerForm.value);

      this.authService.register(this.user).subscribe(
        (response: User) => {
          this.alertify.success('User ' + response.userName + ' was registered !');
        }
        , err => {
          this.alertify.error(err);
        }
        , () => {
          this.authService.login(this.user).subscribe(
            () => {
            this.router.navigate(['/members']);
          });
        }
      );


    }

    console.log(this.registerForm.value);
    console.log('----- REGISTERED.');
  }

  cancel() {
    this.cancelEventFromRegister.emit(false);
    console.log('Cancelled');
  }

}
