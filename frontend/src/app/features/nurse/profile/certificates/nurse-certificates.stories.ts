import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseCertificates } from './nurse-certificates';

const RECORDS = [
  {
    id: 'cert-1',
    name: 'Basic Life Support',
    issuingOrganization: 'American Heart Association',
    issueDate: '2023-03-15',
    expirationDate: '2025-03-15',
    credentialId: 'BLS-998877',
    credentialUrl: 'https://example.org/credentials/bls-998877',
  },
  {
    id: 'cert-2',
    name: 'Advanced Cardiac Life Support',
    issuingOrganization: 'American Heart Association',
    issueDate: '2024-06-01',
    expirationDate: null,
    credentialId: null,
    credentialUrl: null,
  },
  {
    id: 'cert-3',
    name: 'Infection Control Certificate',
    issuingOrganization: 'Harbor Institute',
    issueDate: null,
    expirationDate: null,
    credentialId: null,
    credentialUrl: 'https://example.org/credentials/icc-112233',
  },
];

class PopulatedApi {
  listCertificates() {
    return of(RECORDS);
  }
  createCertificate() {
    return of(RECORDS[0]);
  }
  updateCertificate() {
    return of(RECORDS[0]);
  }
  deleteCertificate() {
    return of(undefined);
  }
}

class EmptyApi extends PopulatedApi {
  override listCertificates() {
    return of([]);
  }
}

function providers(api: PopulatedApi) {
  return [provideRouter([]), { provide: NurseProfileApi, useValue: api }];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const meta: Meta<NurseCertificates> = {
  component: NurseCertificates,
  title: 'Features/Nurse/Certificates',
};
export default meta;
type Story = StoryObj<NurseCertificates>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const AddCertificate: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const add = canvasElement.querySelector<HTMLElement>('[data-testid="add-certificate"]');
    if (add === null) {
      throw new Error('Add certificate story could not find the Add certificate action.');
    }
    add.click();
    await settle();
  },
};

export const EditCertificate: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const edit = canvasElement.querySelector<HTMLElement>('[data-testid="certificate-edit"]');
    if (edit === null) {
      throw new Error('Edit certificate story could not find an Edit action.');
    }
    edit.click();
    await settle();
  },
};

export const DeleteConfirmation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const remove = canvasElement.querySelector<HTMLElement>('[data-testid="certificate-delete"]');
    if (remove === null) {
      throw new Error('Delete confirmation story could not find a Delete action.');
    }
    remove.click();
    await settle();
  },
};
