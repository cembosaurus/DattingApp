import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        return next.handle(req)
            .pipe(
                catchError(err => {

console.warn(err);

                        if (err instanceof HttpErrorResponse) {

                            // ... UNAUTHORIZED ...
                            if (err.status === 401) {
                                return throwError(err.statusText);
                            }


                            // ... CUSTOM ...
                            const customError = err.headers.get('Shit-Happens');

                            if (customError) {

                                console.error(customError);
                                return throwError(customError);

                            }


                            // ... BAD REQUEST ...
                            if (err.status === 400) {

                                // ... MODELSTATE (f.e.: empty fields) ...
                                if (typeof err.error === 'object' && err.error.errors) {

                                    const serverError = err.error.errors;
                                    let modelStateErrors = '';

                                    for (const key in serverError) {

                                        if (serverError[key]) {

                                            modelStateErrors += serverError[key] + '\n';

                                        }

                                    }

                                    return throwError(modelStateErrors || serverError );

                                }

                                // ... f.e.: registering user who already exist in DB ...
                                return throwError(err.error);

                            }

                            return throwError('Server Error');

                        }
                    }
                )
            );
    }
}


export const ErrorInterceptorProvider = {
    provide: HTTP_INTERCEPTORS,
    useClass: ErrorInterceptor,
    multi: true
};
