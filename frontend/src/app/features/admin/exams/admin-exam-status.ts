export function adminExamStatusLabel(
  status: string,
  labels: { draft: string; published: string; archived: string; unavailable: string },
): string {
  switch (status) {
    case 'Draft': return labels.draft;
    case 'Published': return labels.published;
    case 'Archived': return labels.archived;
    default: return labels.unavailable;
  }
}
