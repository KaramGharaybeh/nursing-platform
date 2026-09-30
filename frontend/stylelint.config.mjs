export default {
  extends: ['stylelint-config-standard-scss'],
  rules: {
    'no-empty-source': null,
    'property-disallowed-list': [
      'margin-left',
      'margin-right',
      'padding-left',
      'padding-right',
      'left',
      'right',
    ],
    'declaration-property-value-disallowed-list': {
      float: ['left', 'right'],
      'text-align': ['left', 'right'],
    },
  },
};
