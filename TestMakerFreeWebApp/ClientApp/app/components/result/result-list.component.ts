﻿import { Component, OnChanges, Inject, Input, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'result-list',
    templateUrl: './result-list.component.html',
    styleUrls: ['./result-list.component.css']
})
export class ResultListComponent implements OnChanges {
    @Input() quiz: Quiz;
    results: Result[];
    title: string;

    constructor(private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string,
        private router: Router) {
        this.results = [];
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (typeof changes['quiz'] !== 'undefined') {
            const change = changes['quiz'];
            if (!change.isFirstChange()) {
                this.loadData();
            }
        }
    }

    loadData() {
        const url = this.baseUrl + `api/result/all/${this.quiz.Id}`;
        this.http.get<Result[]>(url).subscribe(result => {
            this.results = result;
        },
            error => console.error(error));
    }

    onCreate() {
        this.router.navigate(['/result/create', this.quiz.Id]);
    }

    onEdit(result: Result) {
        this.router.navigate(['/result/edit', result.Id]);
    }

    onDelete(result: Result) {
        if (confirm('Do you really want to delete this result?')) {
            const url = this.baseUrl + `api/result/${result.Id}`;
            this.http.delete(url).subscribe(res => {
                this.loadData();
            },
                error => console.error(error));
        }
    }
}