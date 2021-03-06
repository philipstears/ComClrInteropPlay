// ComConsoleApp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#import "../ComCalculator/Debug/ComCalculator.tlb" no_namespace

int _tmain(int argc, _TCHAR* argv[])
{
	CoInitialize(NULL);

	// Spin up the application
	CComPtr<ICalculatorApplication> application;
	application.CoCreateInstance(__uuidof(CalculatorApplication));

	// Load the plugin
	auto plugin1 = application->GetPlugin(OLESTR("ComPlugin.MyCalculator"));
	
	// Get it to do some work
	auto result = plugin1->Add(1, 5);

	return 0;
}

