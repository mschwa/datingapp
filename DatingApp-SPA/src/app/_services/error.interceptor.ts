import { Injectable } from '../../../node_modules/@angular/core';
import { HttpErrorResponse, HTTP_INTERCEPTORS } from '../../../node_modules/@angular/common/http';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent} from '../../../node_modules/@angular/common/http';
import { Observable, throwError } from '../../../node_modules/rxjs';
import { catchError} from '../../../node_modules/rxjs/operators';

@Injectable()

export class ErrorInterceptor implements HttpInterceptor {

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse) {
                    if (error.status === 401) {
                        return throwError(error.statusText);
                    }
                    const applicatoinError = error.headers.get('Application-Error');
                    if (applicatoinError) {
                        return throwError(applicatoinError);
                    }
                }
                const serverError = error.error;
                let modelStateErrors = '';
                if (serverError && typeof serverError === 'object') {
                    for (const key in serverError) {
                        if (serverError[key]) {
                            modelStateErrors += serverError[key] + '\n';
                        }
                    }
                }
                return throwError(modelStateErrors || serverError || 'Server Error');
            })
        );
    }
}

export const ErrorInterceptorProvider = {
    provide: HTTP_INTERCEPTORS,
    useClass: ErrorInterceptor,
    multi: true
};
