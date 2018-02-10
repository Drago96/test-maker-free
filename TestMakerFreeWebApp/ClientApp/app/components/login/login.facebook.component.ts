import { Component, Inject, OnInit, NgZone, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

declare let window: any;
declare let FB: any;

@Component({
    selector: 'login-facebook',
    templateUrl: './login.facebook.component.html'
})
export class LoginFacebookComponent implements OnInit {
    constructor(private http: HttpClient,
        private router: Router,
        private authService: AuthService,
        private zone: NgZone,
        @Inject(PLATFORM_ID) private platformId: any,
        @Inject('BASE_URL') private baseUrl: string) {
    }

    ngOnInit(): void {
        if (!isPlatformBrowser(this.platformId)) {
            return;
        }

        if (typeof (FB) === 'undefined') {
            window.fbAsyncInit = () =>

                this.zone.run(() => {
                    FB.init({
                        appId: '147345425952497',
                        xfbml: true,
                        version: 'v2.10'
                    });
                    FB.AppEvents.logPageView();

                    FB.Event.subscribe('auth.statusChange', (
                            (result: any) => {
                                if (result.status === 'connected') {
                                    this.onConnect(result.authResponse.accessToken);
                                }
                            })
                    );
                });

                // Load the SDK js library (only once)
                (((d, s, id) => {
                    let fjs = d.getElementsByTagName(s)[0];
                    if (d.getElementById(id)) {
                        return;
                    }
                    let js: any = d.createElement(s);
                    js.id = id;
                    (<any>js).src = "//connect.facebook.net/en_US/sdk.js";
                    fjs.parentNode!.insertBefore(js, fjs);
                })(document, 'script', 'facebook-jssdk'));
        } else {
            window.FB.XFBML.parse();

            FB.getLoginStatus((response: any) => {
                if (response.status === 'connected') {
                    FB.logout((res: any) => {
                    });
                }
            });
        }
    }

    onConnect(accessToken: string) {
        const url = this.baseUrl + 'api/token/facebook';
        const data = {
            access_token: accessToken,
            client_id: this.authService.clientId
        };
        this.http.post<TokenResponse>(url, data)
            .subscribe((result) => {
                    if (result) {
                        this.authService.setAuth(result);
                        this.router.navigate(['home']);
                    }
                },
                error => console.error(error));
    }
}