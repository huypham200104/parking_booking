import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * CtaBannerSectionComponent – SRP: Full-width conversion CTA.
 * Single intent: drive user to booking.
 */
@Component({
  selector: 'app-cta-banner-section',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './cta-banner-section.html',
  styleUrl: './cta-banner-section.scss',
})
export class CtaBannerSectionComponent {}
