﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Features.Intellisense.Options;
using JetBrains.UI.Controls;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Extensions;
using JetBrains.UI.Options;
using JetBrains.UI.Options.OptionsDialog2.SimpleOptions;
using JetBrains.UI.Wpf.Controls;
using JetBrains.UI.Wpf.Converters;

namespace GammaJul.ReSharper.EnhancedTooltip.Settings {

	[OptionsPage(Pid, "Enhanced Tooltip", typeof(ThemedIcons.Logo16), ParentId = IntelliSensePage.PID, Sequence = 100)]
	public partial class EnhancedTooltipOptionsPage : IOptionsPage, ISearchablePage {

		public const string Pid = "EnhancedTooltip.OptionsPage";

		[NotNull] private readonly Lifetime _lifetime;
		[NotNull] private readonly OptionsSettingsSmartContext _context;
		[NotNull] private readonly List<OptionsPageKeyword> _keywords = new List<OptionsPageKeyword>();

		public string Id
			=> Pid;

		public bool OnOk()
			=> true;

		public bool ValidatePage()
			=> true;

		public EitherControl Control
			=> this;

		public IProperty<OptionsFilterResult> SearchFilter { get; }
			= new Property<OptionsFilterResult>(String.Format(CultureInfo.InvariantCulture, "Filter for {0}", typeof(EnhancedTooltipOptionsPage)));

		public OptionsPageKeywords GetKeywords()
			=> new OptionsPageKeywords(_keywords);

		public void HighLightKeyword(OptionsFilterResult text)
			=> SearchFilter.Value = text;

		public IEnumerable<string> GetTagKeywordsForPage()
			=> new[] { "Tooltip" };

		[NotNull]
		private static List<EnumValue> CreateEnumItemsSource([NotNull] Type enumType) {
			var enumDescriptionConverter = new EnumDescriptionConverter();
			return Enum.GetValues(enumType)
				.Cast<Enum>()
				.Select(e => new EnumValue(e, enumDescriptionConverter.Convert(e, typeof(string), null, CultureInfo.CurrentUICulture) as string ?? e.ToString()))
				.ToList();
		}

		private void SetCheckBoxBinding<TSettings>(
			[NotNull] Expression<Func<TSettings, bool>> settingAccessor,
			[NotNull] CheckBoxDisabledNoCheck2 checkBox,
			[CanBeNull] CheckBoxDisabledNoCheck2 parentCheckBox,
			bool addColonToDescription = false) {

			var entry = _context.Schema.GetScalarEntry(settingAccessor);
			if (checkBox.Content == null) {
				string description = entry.Description;
				_keywords.Add(new OptionsPageKeyword(description));
				if (addColonToDescription)
					description += ":";
				checkBox.Content = description;
			}
			_context.SetBinding<bool>(_lifetime, entry, checkBox, CheckBoxDisabledNoCheck2.IsCheckedLogicallyDependencyProperty);
			SearchablePageBehavior.SetSearchFilter(checkBox, true);

			parentCheckBox?.IsAppearingChecked.FlowInto(_lifetime, checkBox, IsEnabledProperty);
		}

		private void SetComboBoxBinding<TSettings, TEnum>(
			[NotNull] Expression<Func<TSettings, TEnum>> settingAccessor,
			[NotNull] ComboBox comboBox,
			[CanBeNull] CheckBoxDisabledNoCheck2 parentCheckBox) {

			comboBox.ItemsSource = CreateEnumItemsSource(typeof(TEnum));
			comboBox.DisplayMemberPath = nameof(EnumValue.Description);
			comboBox.SelectedValuePath = nameof(EnumValue.Value);

			SettingsScalarEntry entry = _context.Schema.GetScalarEntry(settingAccessor);
			_context.SetBinding<object>(_lifetime, entry, comboBox, Selector.SelectedValueProperty);

			SetAssociatedLabel(comboBox, entry.Description);
			
			parentCheckBox?.IsAppearingChecked.FlowInto(_lifetime, comboBox, IsEnabledProperty);
		}

		private void SetNumericUpDownBinding<TSettings>(
			[NotNull] Expression<Func<TSettings, int>> settingAccessor,
			[NotNull] NumericUpDown numericUpDown,
			[CanBeNull] CheckBoxDisabledNoCheck2 parentCheckBox) {

			SettingsScalarEntry entry = _context.Schema.GetScalarEntry(settingAccessor);
			_context.SetBinding<int>(_lifetime, entry, numericUpDown, NumericUpDown.ValueProperty);

			SetAssociatedLabel(numericUpDown, entry.Description);

			parentCheckBox?.IsAppearingChecked.FlowInto(_lifetime, numericUpDown, IsEnabledProperty);
		}

		private void SetAssociatedLabel([NotNull] FrameworkElement element, [NotNull] string description) {
			Label label = element.Parent?.FindDescendant<Label>();
			if (label == null)
				return;

			if (label.Content == null) {
				label.Content = description + ":";
				_keywords.Add(new OptionsPageKeyword(description));
			}
			else {
				string existingContentString = label.Content as string;
				if (existingContentString != null)
					_keywords.Add(new OptionsPageKeyword(existingContentString));
			}

			SearchablePageBehavior.SetSearchFilter(label, true);
		}

		private void SetIdentifierTooltipSettingsBindings() {
			CheckBoxDisabledNoCheck2 rootCheckBox = IdentifierTooltipEnabledCheckBox;
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.Enabled, rootCheckBox, null);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowIcon, IdentifierTooltipShowIconCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowKind, IdentifierTooltipShowKindCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.UseExtensionMethodKind, IdentifierTooltipUseExtensionMethodKindCheckBox, IdentifierTooltipShowKindCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.UseAttributeClassKind, IdentifierTooltipUseAttributeClassKindCheckBox, IdentifierTooltipShowKindCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.UseMethodModifiersInKind, IdentifierTooltipUseMethodModifiersInKindCheckBox, IdentifierTooltipShowKindCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.UseClassModifiersInKind, IdentifierTooltipUseClassModifiersInKindCheckBox, IdentifierTooltipShowKindCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowObsolete, IdentifierTooltipShowObsoleteCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowExceptions, IdentifierTooltipShowExceptionsCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowOverloadCount, IdentifierTooltipShowOverloadCountCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowArgumentsRole, IdentifierTooltipShowArgumentsRoleCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowBaseType, IdentifierTooltipShowBaseTypeCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowImplementedInterfaces, IdentifierTooltipShowImplementedInterfacesCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.ShowAttributesUsage, IdentifierTooltipShowShowAttributesUsageCheckBox, rootCheckBox);
			SetCheckBoxBinding((IdentifierTooltipSettings s) => s.UseTypeKeywords, IdentifierTooltipUseTypeKeywordsCheckBox, rootCheckBox);
			SetComboBoxBinding((IdentifierTooltipSettings s) => s.ShowIdentifierAnnotations, IdentifierTooltipShowIdentifierAnnotationsComboBox, rootCheckBox);
			SetComboBoxBinding((IdentifierTooltipSettings s) => s.ShowParametersAnnotations, IdentifierTooltipShowParametersAnnotationsComboBox, rootCheckBox);
			SetComboBoxBinding((IdentifierTooltipSettings s) => s.ConstructorReferenceDisplay, IdentifierTooltipConstructorReferenceDisplayComboBox, rootCheckBox);
			SetComboBoxBinding((IdentifierTooltipSettings s) => s.AttributeConstructorReferenceDisplay, IdentifierTooltipAttributeConstructorReferenceDisplayComboBox, rootCheckBox);
		}

		private void SetIssueTooltipSettingsBindings() {
			SetCheckBoxBinding((IssueTooltipSettings s) => s.ShowIcon, IssueTooltipShowIconCheckBox, null);
			SetCheckBoxBinding((IssueTooltipSettings s) => s.ColorizeElementsInErrors, IssueTooltipColorizeElementsInErrors, null);
		}

		private void SetParameterInfoSettingsBindings() {
			CheckBoxDisabledNoCheck2 rootCheckBox = ParameterInfoEnabledCheckBox;
			SetCheckBoxBinding((ParameterInfoSettings s) => s.Enabled, rootCheckBox, null);
			SetCheckBoxBinding((ParameterInfoSettings s) => s.ShowEmptyParametersText, ParameterInfoShowEmptyParametersTextCheckBox, rootCheckBox);
			SetCheckBoxBinding((ParameterInfoSettings s) => s.UseTypeKeywords, ParameterInfoUseTypeKeywordsCheckBox, rootCheckBox);
		}

		private void SetDisplaySettingsBindings() {
			CheckBoxDisabledNoCheck2 rootCheckBox = DisplayLimitTooltipWidth;
			SetCheckBoxBinding((DisplaySettings s) => s.LimitTooltipWidth, rootCheckBox, null, addColonToDescription: true);
			SetNumericUpDownBinding((DisplaySettings s) => s.ScreenWidthLimitPercent, DisplayScreenWidthLimitPercent, rootCheckBox);
			SetComboBoxBinding((DisplaySettings s) => s.TextFormattingMode, DisplayTextFormattingMode, null);
		}

		public EnhancedTooltipOptionsPage([NotNull] Lifetime lifetime, [NotNull] OptionsSettingsSmartContext context) {
			_lifetime = lifetime;
			_context = context;

			DataContext = this;
			InitializeComponent();

			SetIdentifierTooltipSettingsBindings();
			SetIssueTooltipSettingsBindings();
			SetParameterInfoSettingsBindings();
			SetDisplaySettingsBindings();
		}

	}

}