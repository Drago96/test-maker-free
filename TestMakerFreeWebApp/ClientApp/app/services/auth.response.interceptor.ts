import { Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import {
    HttpClient,
    HttpHandler, HttpEvent, HttpInterceptor,
    HttpRequest, HttpResponse, HttpErrorResponse
} from '@angular/common/http';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';

@Injectable()
export class AuthResponseInterceptor implements HttpInterceptor {
    currentRequest: HttpRequest<any>;
    auth: AuthService;

    constructor(private injector: Injector,
        private router: Router) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        this.auth = this.injector.get(AuthService);
        const token = (this.auth.isLoggedIn()) ? this.auth.getAuth()!.token : null;

        if (token) {
            this.currentRequest = req;

            return next.handle(req)
                .do((event: HttpEvent<any>) => {
                    if (event instanceof HttpResponse) {
                    }
                }).catch(error => {
                    return this.handleError(error, next);
                });
        } else {
            return next.handle(req);
        }
    }

    handleError(error: any, next: HttpHandler) {
        if (error instanceof HttpErrorResponse) {
            if (error.status === 401) {
                var previousRequest = this.currentRequest;
                return this.auth.refreshToken()
                    .flatMap((refreshed) => {
                        const token = (this.auth.isLoggedIn()) ? this.auth.getAuth()!.token : null;
                        if (token) {
                            previousRequest = previousRequest.clone({
                                setHeaders: { Authorization: `Bearer ${token}` }
                            });
                        }
                        return next.handle(previousRequest);
                    });
            }
        }
        return Observable.throw(error);
    }
}