import { Component, AfterViewInit } from '@angular/core';
import { HeroSectionComponent } from '../../components/hero-section/hero-section';
import { StatsStripComponent } from '../../components/stats-strip/stats-strip';
import { FeaturesSectionComponent } from '../../components/features-section/features-section';
import { HowItWorksSectionComponent } from '../../components/how-it-works-section/how-it-works-section';
import { TestimonialsSectionComponent } from '../../components/testimonials-section/testimonials-section';
import { CtaBannerSectionComponent } from '../../components/cta-banner-section/cta-banner-section';

/**
 * HomeComponent – OCP + SRP:
 * Composes all section components and manages page-level scroll reveals.
 * Each section only defines markup; this orchestrator sets up IntersectionObserver
 * so individual sections stay free of Observer boilerplate (DIP).
 */
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    HeroSectionComponent,
    StatsStripComponent,
    FeaturesSectionComponent,
    HowItWorksSectionComponent,
    TestimonialsSectionComponent,
    CtaBannerSectionComponent,
  ],
  template: `
    <app-hero-section />
    <app-stats-strip />
    <app-features-section />
    <app-how-it-works-section />
    <app-testimonials-section />
    <app-cta-banner-section />
  `,
})
export class HomeComponent implements AfterViewInit {

  ngAfterViewInit(): void {
    this.initScrollReveal();
  }

  /**
   * Sets up IntersectionObserver for ALL .reveal elements on the page.
   * Uses requestAnimationFrame to wait for child components to render.
   */
  private initScrollReveal(): void {
    requestAnimationFrame(() => {
      const observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) {
              entry.target.classList.add('visible');
              observer.unobserve(entry.target); // fire once
            }
          });
        },
        { threshold: 0.08, rootMargin: '0px 0px -40px 0px' }
      );

      document.querySelectorAll('.reveal').forEach((el) => observer.observe(el));
    });
  }
}
