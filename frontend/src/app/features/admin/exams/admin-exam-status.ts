export function adminExamStatusLabel(status: string): string {
  switch (status) {
    case 'Draft': return 'Draft';
    case 'Published': return 'Published';
    case 'Archived': return 'Archived';
    default: return 'Status unavailable';
  }
}
