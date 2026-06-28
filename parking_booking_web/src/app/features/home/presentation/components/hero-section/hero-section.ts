import { Component, AfterViewInit, ElementRef, ViewChildren, QueryList, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

/**
 * HeroSectionComponent – SRP: Only handles the hero fold display.
 * Split-screen layout: left = copy + search CTA, right = parking image.
 */
@Component({
  selector: 'app-hero-section',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './hero-section.html',
  styleUrl: './hero-section.scss',
})
export class HeroSectionComponent implements AfterViewInit {
  private router = inject(Router);
  @ViewChildren('revealEl') revealElements!: QueryList<ElementRef>;

  searchQuery = '';

  ngAfterViewInit(): void {
    // Intersection Observer for scroll-reveal (no window.scroll listener)
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.15 }
    );

    this.revealElements.forEach((el) => observer.observe(el.nativeElement));
  }

  onSearch(): void {
    // Pass search query as query param to map page
    this.router.navigate(['/map'], { queryParams: { q: this.searchQuery } });
  }
}
