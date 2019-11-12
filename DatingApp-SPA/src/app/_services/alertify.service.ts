import { Injectable } from '@angular/core';

import * as alertify from 'alertifyjs';
// ...... OR:
// declare let alertify: any;

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

  constructor() { }


  confirm(message: string, OKCallback: () => any) {

    alertify.confirm(
      message,
      function(e) {

        if (e) {
          OKCallback();
        } else {}

      }
    );
  }


  success(message: string) {

    alertify.success(message);

  }

  error(message: string) {

    alertify.error(message);

  }


  warning(message: string) {

    alertify.warning(message);

  }


  message(message: string) {

    alertify.message(message);

  }


}
