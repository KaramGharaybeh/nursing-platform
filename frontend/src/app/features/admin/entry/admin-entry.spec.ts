import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import type { CurrentUser } from '../../../core/auth/current-user';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { AdminEntry } from './admin-entry';

const READY_ADMIN: CurrentUser = {
  id: 'admin-1',
  email: 'admin@example.com',
  username: 'adminuser',
  firstName: 'Ada',
  lastName: 'Admin',
  isActive: true,
  emailVerified: true,
  isProfileComplete: true,
  roles: ['Admin'],
  permissions: [
    'Users.View',
    'Exams.View',
    'Questions.View',
    'ReportingTopics.Manage',
    'PracticeCollections.Manage',
  ],
  createdAt: '2026-09-23T00:00:00Z',
  lastLoginAt: undefined,
};

class CurrentUserStoreStub {
  readonly status = vi.fn(() => 'ready' as const);
  readonly currentUser = vi.fn(() => READY_ADMIN);
}

async function setup(store = new CurrentUserStoreStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [AdminEntry],
    providers: [provideRouter([]), { provide: CurrentUserStore, useValue: store }],
  }).compileComponents();
  const fixture = TestBed.createComponent(AdminEntry);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return { fixture, store };
}

describe('AdminEntry', () => {
  it('renders a static Admin workspace with eligible canonical destinations only', async () => {
    const { fixture } = await setup();
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('a'));

    expect(text).toContain('Admin workspace');
    expect(text).toContain('Choose an approved administrative workspace.');
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      canonicalRoutePath('ADMIN_USERS'),
      canonicalRoutePath('ADMIN_REFERENCE_DATA'),
      canonicalRoutePath('ADMIN_EXAMS'),
      canonicalRoutePath('ADMIN_EXAM_QUESTIONS'),
      canonicalRoutePath('ADMIN_PAYMENT_PRODUCTS'),
      canonicalRoutePath('ADMIN_PREPARATION_PACKAGE_TOPICS'),
      canonicalRoutePath('ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS'),
    ]);
    expect(text).toContain('Users');
    expect(text).toContain('Exam categories');
    expect(text).toContain('Exams');
    expect(text).toContain('Questions');
    expect(text).toContain('Payment products');
    expect(text).toContain('Reporting topics');
    expect(text).toContain('Practice collections');
    expect(text).not.toContain('Reporting profiles');
    expect(text).not.toContain('Study materials');
    expect(text).not.toContain('Package definitions');
    expect(text).not.toContain('Package offers');
  });

  it('filters Admin destinations through the navigation permission policy without admin bypass', async () => {
    const limitedStore = new CurrentUserStoreStub();
    limitedStore.currentUser.mockReturnValue({
      ...READY_ADMIN,
      permissions: ['Users.View'],
    });

    const { fixture } = await setup(limitedStore);
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('a'));

    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      canonicalRoutePath('ADMIN_USERS'),
    ]);
    expect(text).toContain('Users');
    expect(text).not.toContain('Exams');
    expect(text).not.toContain('Questions');
    expect(text).not.toContain('Payment products');
  });

  it('does not expose dashboard metrics, blocked Admin areas, or fake operational content', async () => {
    const { fixture } = await setup();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    const forbidden = [
      'Dashboard',
      'KPI',
      'statistics',
      'metrics',
      'recent activity',
      'system health',
      'alerts',
      'revenue',
      'payment orders',
      'recruitment',
      'roles and permissions',
      'total users',
      'total exams',
    ];

    for (const value of forbidden) {
      expect(text.toLowerCase()).not.toContain(value.toLowerCase());
    }
  });
});
