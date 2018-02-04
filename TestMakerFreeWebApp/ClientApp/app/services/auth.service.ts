import { EventEmitter, Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import 'rxjs/Rx';

@Injectable()
export class AuthService {
    authKey: string = "auth";
    clientId: string = "TestMakerFree";

    constructor(private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string,
        @Inject(PLATFORM_ID) private platformId: any) {

    }

    login(username: string, password: string): Observable<boolean> {
        const url = this.baseUrl + 'api/token/auth';
        const data = {
            username: username,
            password: password,
            client_id: this.clientId,
            grant_type: 'password',
            scope: 'offline_access profile email'
        };

        return this.http.post<TokenResponse>(url, data)
            .map((res) => {
                const token = res && res.token;

                if (token) {
                    this.setAuth(res);
                    return true;
                }

                return Observable.throw('Unauthorized');
            })
            .catch(error => {
                return Observable.throw(error);
            });
    }

    logOut(): boolean {
        this.setAuth(null);
        return true;
    }

    setAuth(auth: TokenResponse | null): boolean {
        if (isPlatformBrowser(this.platformId)) {
            if (auth) {
                localStorage.setItem(
                    this.authKey,
                    JSON.stringify(auth));
            } else {
                localStorage.removeItem(this.authKey);
            }
        }

        return true;
    }


    getAuth(): TokenResponse | null {
        if (isPlatformBrowser(this.platformId)) {
            const auth = localStorage.getItem(this.authKey);
            if (auth) {
                return JSON.parse(auth);
            }
        }
        return null;
    }

    isLoggedIn(): boolean {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem(this.authKey) !== null;
        }
        return false;
    }

}