import { Component, OnInit, Inject, NgZone, PLATFORM_ID } from '@angular//core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

declare let window: any;

@Component({
    selector: 'login-externalproviders',
    templateUrl: './login.externalproviders.component.html'
})
export class LoginExternalProvidersComponent implements OnInit {
    externalProviderWindow: any;

    constructor(private http:HttpClient,
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

        // close previously opened windows (if any)
        this.closePopUpWindow();

        // instantiate the externalProviderLogin function
        // (if it doesn't exist already)
        if (!window.externalProviderLogin) {
            window.externalProviderLogin = (auth: TokenResponse) => {
                this.zone.run(() => {
                    this.authService.setAuth(auth);
                    this.router.navigate([""]);
                });
            }
        }
    }

    closePopUpWindow() {
        if (this.externalProviderWindow) {
            this.externalProviderWindow.close();
        }
        this.externalProviderWindow = null;
    }

    callExternalLogin(providerName: string) {
        if (!isPlatformBrowser(this.platformId)) {
            return;
        }

        const url = this.baseUrl + 'api/token/externalLogin/' + providerName;

        const w = (screen.width >= 1050) ? 1050 : screen.width;
        const h = (screen.width >= 550) ? 550 : screen.height;

        const  params = 'toolbar=yes,scrollbars=yes,resizable=yes,width=' + w + ', height=' + h;
        // close previously opened windows (if any)
        this.closePopUpWindow();
        this.externalProviderWindow = window.open(url, "ExternalProvider", params, false);
    }
}